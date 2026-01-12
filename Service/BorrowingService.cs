// <copyright file="BorrowingService.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Service
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data.Repositories;
    using Domain.Models;
    using log4net;

    /// <summary>
    /// Service implementation for borrowing operations with business rules validation.
    /// Enforces all constraints defined in <see cref="LibraryConfiguration"/>.
    /// </summary>
    public class BorrowingService : IBorrowingService
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BorrowingService));

        private readonly IBorrowing borrowingRepository;
        private readonly IBook bookRepository;
        private readonly IReader readerRepository;
        private readonly LibraryConfiguration configRepository;

        private readonly IBookDomain bookDomainRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BorrowingService"/> class.
        /// </summary>
        /// <param name="borrowingRepository">The borrowing repository.</param>
        /// <param name="bookRepository">The book repository.</param>
        /// <param name="readerRepository">The reader repository.</param>
        /// <param name="configRepository">The library configuration repository.</param>
        /// <param name="bookDomainRepository">The book domain repository.</param>
        public BorrowingService(
            IBorrowing borrowingRepository,
            IBook bookRepository,
            IReader readerRepository,
            LibraryConfiguration configRepository,
            IBookDomain bookDomainRepository)
        {
            this.borrowingRepository = borrowingRepository ?? throw new ArgumentNullException(nameof(borrowingRepository));
            this.bookRepository = bookRepository ?? throw new ArgumentNullException(nameof(bookRepository));
            this.readerRepository = readerRepository ?? throw new ArgumentNullException(nameof(readerRepository));
            this.configRepository = configRepository ?? throw new ArgumentNullException(nameof(configRepository));

            this.bookDomainRepository = bookDomainRepository ?? throw new ArgumentNullException(nameof(bookDomainRepository));
            this.bookDomainRepository = bookDomainRepository;
        }

        /// <summary>
        /// Attempts to borrow a book with comprehensive business rule validation.
        /// Also records the staff member that processed the borrowing.
        /// </summary>
        /// <param name="readerId">The reader ID.</param>
        /// <param name="bookId">The book ID.</param>
        /// <param name="borrowingDays">The number of days to borrow the book for.</param>
        /// <param name="staffId">The staff member id.</param>
        public void BorrowBook(int readerId, int bookId, int borrowingDays, int staffId)
        {
            this.BorrowBookInternal(readerId, bookId, borrowingDays, staffId);
        }

        /// <summary>
        /// Creates borrowing records for multiple books with comprehensive validation.
        /// </summary>
        /// <param name="readerId">The reader ID.</param>
        /// <param name="bookIds">The book IDs to borrow.</param>
        /// <param name="borrowingDate">The borrowing date.</param>
        /// <param name="daysToBorrow">The number of days to borrow each book for.</param>
        public void CreateBorrowings(int readerId, List<int> bookIds, DateTime borrowingDate, int daysToBorrow)
        {
            throw new InvalidOperationException("StaffId is required to create borrowings.");
        }

        /// <summary>
        /// Creates borrowing records for multiple books with comprehensive validation.
        /// Also records the staff member that processed the borrowings.
        /// </summary>
        /// <param name="readerId">Reader id.</param>
        /// <param name="bookIds">Book ids.</param>
        /// <param name="borrowingDate">Borrowing date.</param>
        /// <param name="daysToBorrow">Days to borrow.</param>
        /// <param name="staffId">Staff member id.</param>
        public void CreateBorrowings(int readerId, List<int> bookIds, DateTime borrowingDate, int daysToBorrow, int staffId)
        {
            this.CreateBorrowingsInternal(readerId, bookIds, borrowingDate, daysToBorrow, staffId);
        }

        /// <summary>
        /// Returns a borrowed book.
        /// </summary>
        /// <param name="borrowingId">The borrowing ID.</param>
        /// <param name="returnDate">The return date.</param>
        public void ReturnBorrowing(int borrowingId, DateTime returnDate)
        {
            var borrowing = this.borrowingRepository.GetById(borrowingId);
            if (borrowing == null)
            {
                throw new InvalidOperationException("Borrowing record not found.");
            }

            if (!borrowing.IsActive)
            {
                throw new InvalidOperationException("This borrowing record is already returned.");
            }

            borrowing.ReturnDate = returnDate;
            borrowing.IsActive = false;

            try
            {
                this.borrowingRepository.Update(borrowing);
                Logger.InfoFormat("ReturnBorrowing succeeded. BorrowingId={0}, ReturnDate={1}", borrowingId, returnDate);
            }
            catch (Exception ex)
            {
                Logger.Error("ReturnBorrowing failed.", ex);
                throw new InvalidOperationException($"Failed to return borrowing record: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extends a borrowing period if allowed.
        /// </summary>
        /// <param name="borrowingId">The borrowing ID.</param>
        /// <param name="extensionDays">The number of extension days.</param>
        /// <param name="extensionDate">The extension date.</param>
        public void ExtendBorrowing(int borrowingId, int extensionDays, DateTime extensionDate)
        {
            var borrowing = this.borrowingRepository.GetById(borrowingId);
            if (borrowing == null)
            {
                throw new InvalidOperationException("Borrowing record not found.");
            }

            if (!borrowing.IsActive)
            {
                throw new InvalidOperationException("Cannot extend a returned borrowing record.");
            }

            var reader = this.readerRepository.GetById(borrowing.ReaderId);
            if (reader == null)
            {
                throw new InvalidOperationException("Reader not found.");
            }

            var config = this.configRepository;
            var maxExtensionDays = config.MaxExtensionDays;

            // Validation 1: Check total extension limit
            if (borrowing.TotalExtensionDays + extensionDays > maxExtensionDays)
            {
                throw new InvalidOperationException(
                    $"Extension would exceed limit of {maxExtensionDays} days.");
            }

            // Validation 2: Verify book is still available
            var book = this.bookRepository.GetById(borrowing.BookId);
            if (book == null)
            {
                throw new InvalidOperationException("Book not found.");
            }

            if (book.GetAvailableCopies() <= 0)
            {
                throw new InvalidOperationException("Book is no longer available for extension.");
            }

            // Validation 3: Check minimum available percentage
            var availablePercentage = (double)book.GetAvailableCopies() / book.TotalCopies;
            if (availablePercentage < config.MinAvailablePercentage)
            {
                throw new InvalidOperationException(
                    "Cannot extend borrowing. Book availability below minimum threshold.");
            }

            // Validation 4: Check 3-month extension limit (LIM in last 3 months)
            var threeMonthsAgo = extensionDate.AddMonths(-3);
            var borrowingsInLastThreeMonths = this.borrowingRepository.GetBorrowingsByDateRange(
                threeMonthsAgo,
                extensionDate);

            var totalExtensionInPeriod = borrowingsInLastThreeMonths
                .Where(b => b.ReaderId == borrowing.ReaderId)
                .Sum(b => b.TotalExtensionDays);

            var maxExtensionInThreeMonths = reader.IsStaff
                ? maxExtensionDays * 2
                : maxExtensionDays;

            if (totalExtensionInPeriod + extensionDays > maxExtensionInThreeMonths)
            {
                throw new InvalidOperationException(
                    $"Cannot exceed {maxExtensionInThreeMonths} extension days in 3 months.");
            }

            borrowing.DueDate = borrowing.DueDate.AddDays(extensionDays);
            borrowing.TotalExtensionDays += extensionDays;
            borrowing.LastExtensionDate = extensionDate;

            try
            {
                this.borrowingRepository.Update(borrowing);
                Logger.InfoFormat("ExtendBorrowing succeeded. BorrowingId={0}, ExtensionDays={1}", borrowingId, extensionDays);
            }
            catch (Exception ex)
            {
                Logger.Error("ExtendBorrowing failed.", ex);
                throw new InvalidOperationException($"Failed to extend borrowing record: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extends a borrowing period with comprehensive validation (advanced version using current date).
        /// Validates: extension limit within last 3 months, book availability, and minimum percentage.
        /// </summary>
        /// <param name="borrowingId">The borrowing ID.</param>
        /// <param name="extensionDays">The number of extension days.</param>
        public void ExtendBorrowingAdvanced(int borrowingId, int extensionDays)
        {
            this.ExtendBorrowing(borrowingId, extensionDays, DateTime.Now);
        }

        /// <summary>
        /// Gets all active borrowings for a reader.
        /// </summary>
        /// <param name="readerId">The reader ID.</param>
        /// <returns>A collection of active borrowings for the specified reader.</returns>
        public IEnumerable<Borrowing> GetActiveBorrowings(int readerId)
        {
            return this.borrowingRepository.GetActiveBorrowingsByReader(readerId);
        }

        /// <summary>
        /// Gets overdue borrowings.
        /// </summary>
        /// <returns>A collection of overdue borrowings.</returns>
        public IEnumerable<Borrowing> GetOverdueBorrowings()
        {
            return this.borrowingRepository.GetOverdueBorrowings();
        }

        /// <summary>
        /// Validates all business rules for borrowing a book.
        /// </summary>
        /// <param name="readerId">The reader ID.</param>
        /// <param name="bookId">The book ID.</param>
        /// <returns><c>true</c> if the reader can borrow the book; otherwise, <c>false</c>.</returns>
        public bool CanBorrowBook(int readerId, int bookId)
        {
            var config = this.configRepository;
            var reader = this.readerRepository.GetById(readerId);
            var book = this.bookRepository.GetById(bookId);

            if (reader == null || book == null)
            {
                return false;
            }

            if (book.GetAvailableCopies() <= 0)
            {
                return false;
            }

            var loanableStock = book.TotalCopies - book.ReadingRoomOnlyCopies;
            if (loanableStock <= 0)
            {
                return false;
            }

            var availablePercentage = (double)book.GetAvailableCopies() / loanableStock;
            if (availablePercentage < config.MinAvailablePercentage)
            {
                return false;
            }

            var activeBorrowings = this.borrowingRepository.GetActiveBorrowingsByReader(readerId);
            var maxBooks = reader.IsStaff ? config.MaxBooksPerPeriod * 2 : config.MaxBooksPerPeriod;
            if (activeBorrowings.Count() >= maxBooks)
            {
                return false;
            }

            var domainLimitMonths = config.DomainLimitMonths;
            var lastMonthBorrowings = this.borrowingRepository.GetBorrowingsByDateRange(DateTime.Now.AddMonths(-domainLimitMonths), DateTime.Now) ?? Enumerable.Empty<Borrowing>();

            // New: domain limits apply cumulatively across the domain tree.
            // For each ancestor in the candidate book's domain chain, compute the set of descendant domains
            // and count borrowings inside that subtree. If any ancestor subtree count reaches the limit,
            // borrowing is blocked.
            var processedAncestorIds = new HashSet<int>();
            foreach (var domain in book.Domains ?? Enumerable.Empty<BookDomain>())
            {
                // load current domain from repository to be safe
                var current = this.bookDomainRepository.GetById(domain.Id);
                if (current == null)
                {
                    continue;
                }

                // traverse up the ancestor chain and evaluate each ancestor once
                var ancestor = current;
                while (ancestor != null)
                {
                    if (!processedAncestorIds.Add(ancestor.Id))
                    {
                        // already evaluated this ancestor
                        ancestor = ancestor.ParentDomainId.HasValue ? this.bookDomainRepository.GetById(ancestor.ParentDomainId.Value) : null;
                        continue;
                    }

                    // collect descendant ids including the ancestor itself
                    var descendantIds = new List<int> { ancestor.Id };
                    this.GetDescendantDomainIds(ancestor.Id, descendantIds);

                    var domainBooksCount = lastMonthBorrowings.Count(b => b.Book != null && b.Book.Domains.Any(d => descendantIds.Contains(d.Id)));
                    var maxDomainBooks = reader.IsStaff ? config.MaxBooksPerDomain * 2 : config.MaxBooksPerDomain;
                    if (domainBooksCount >= maxDomainBooks)
                    {
                        return false;
                    }

                    ancestor = ancestor.ParentDomainId.HasValue ? this.bookDomainRepository.GetById(ancestor.ParentDomainId.Value) : null;
                }
            }

            var lastBorrowing = this.borrowingRepository.GetBorrowingsByBook(bookId).Where(b => b.ReaderId == readerId)
                .OrderByDescending(b => b.BorrowingDate).FirstOrDefault();

            if (lastBorrowing != null && lastBorrowing.ReturnDate.HasValue)
            {
                var daysSinceReturn = (DateTime.Now - lastBorrowing.ReturnDate.Value).Days;
                var minDaysBetweenBorrows = reader.IsStaff ? config.MinDaysBetweenBorrows / 2 : config.MinDaysBetweenBorrows;

                if (daysSinceReturn < minDaysBetweenBorrows)
                {
                    return false;
                }
            }

            var todayBorrowings = this.borrowingRepository.GetBorrowingsByDateRange(DateTime.Now.Date, DateTime.Now);

            var maxBooksPerDay = reader.IsStaff ? int.MaxValue : config.MaxBooksPerDay;
            if (todayBorrowings.Count() >= maxBooksPerDay)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets all descendant domain IDs recursively.
        /// </summary>
        private void GetDescendantDomainIds(int parentDomainId, List<int> result)
        {
            var subdomains = this.bookDomainRepository.GetSubdomains(parentDomainId);
            foreach (var subdomain in subdomains)
            {
                if (!result.Contains(subdomain.Id))
                {
                    result.Add(subdomain.Id);
                    this.GetDescendantDomainIds(subdomain.Id, result);
                }
            }
        }

        /// <summary>
        /// Gets the count of active borrowings for a reader.
        /// </summary>
        /// <param name="readerId">The reader ID.</param>
        /// <returns>The count of active borrowings for the specified reader.</returns>
        public int GetActiveBorrowingCount(int readerId)
        {
            return this.borrowingRepository.GetActiveBorrowingsByReader(readerId).Count();
        }

        private static void ValidateStaffMember(Reader staff)
        {
            if (staff == null)
            {
                throw new InvalidOperationException("Staff member not found.");
            }

            if (!staff.IsStaff)
            {
                throw new InvalidOperationException("Specified staff member is not marked as staff.");
            }
        }

        private void BorrowBookInternal(int readerId, int bookId, int borrowingDays, int? staffId)
        {
            Logger.InfoFormat("BorrowBook called. readerId={0}, bookId={1}, borrowingDays={2}, staffId={3}", readerId, bookId, borrowingDays, staffId);

            var reader = this.readerRepository.GetById(readerId);
            var book = this.bookRepository.GetById(bookId);

            if (reader == null)
            {
                Logger.WarnFormat("BorrowBook failed: reader not found. readerId={0}", readerId);
                throw new InvalidOperationException("Reader not found.");
            }

            if (book == null)
            {
                Logger.WarnFormat("BorrowBook failed: book not found. bookId={0}", bookId);
                throw new InvalidOperationException("Book not found.");
            }

            if (staffId.HasValue)
            {
                var staff = this.readerRepository.GetById(staffId.Value);
                ValidateStaffMember(staff);

                if (!reader.IsStaff)
                {
                var staffBorrowingsSeq = this.borrowingRepository.GetBorrowingsByDateRange(DateTime.Now.Date, DateTime.Now) ?? Enumerable.Empty<Borrowing>();
                var staffBorrowingsToday = staffBorrowingsSeq.Where(b => b.StaffId.HasValue && b.StaffId.Value == staffId.Value).Count();

                if (staffBorrowingsToday + 1 > this.configRepository.MaxBooksStaffPerDay)
                  {
                        throw new InvalidOperationException(
                            $"Staff cannot distribute more than {this.configRepository.MaxBooksStaffPerDay} books per day.");
                    }
                }
            }

            if (!this.CanBorrowBook(readerId, bookId))
            {
                Logger.WarnFormat("BorrowBook blocked by business rules. readerId={0}, bookId={1}", readerId, bookId);
                throw new InvalidOperationException("Reader cannot borrow this book. Business rules violated.");
            }

            var borrowing = new Borrowing
            {
                ReaderId = readerId,
                StaffId = staffId,
                BookId = bookId,
                BorrowingDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(borrowingDays),
                IsActive = true,
                InitialBorrowingDays = borrowingDays,
                TotalExtensionDays = 0,
            };

            try
            {
                this.borrowingRepository.Add(borrowing);
                Logger.InfoFormat("BorrowBook succeeded. borrowingId={0}", borrowing.Id);
            }
            catch (Exception ex)
            {
                Logger.Error("BorrowBook failed to persist borrowing.", ex);
                throw new InvalidOperationException($"Failed to create borrowing record: {ex.Message}", ex);
            }
        }

        private void CreateBorrowingsInternal(int readerId, List<int> bookIds, DateTime borrowingDate, int daysToBorrow, int? staffId)
        {
            Logger.InfoFormat("CreateBorrowings called. readerId={0}, bookCount={1}, borrowingDate={2}, daysToBorrow={3}, staffId={4}", readerId, bookIds?.Count ?? 0, borrowingDate, daysToBorrow, staffId);

            if (bookIds == null)
            {
                throw new ArgumentNullException(nameof(bookIds));
            }

            var reader = this.readerRepository.GetById(readerId);
            if (reader == null)
            {
                Logger.WarnFormat("CreateBorrowings failed: reader not found. readerId={0}", readerId);
                throw new InvalidOperationException("Reader not found.");
            }

            if (staffId.HasValue)
            {
                var staff = this.readerRepository.GetById(staffId.Value);
                ValidateStaffMember(staff);

                if (!reader.IsStaff)
                {
                    var staffBorrowingsSeq = this.borrowingRepository.GetBorrowingsByDateRange(borrowingDate.Date, borrowingDate) ?? Enumerable.Empty<Borrowing>();
                    var staffBorrowingsToday = staffBorrowingsSeq
                        .Where(b => b.StaffId.HasValue && b.StaffId.Value == staffId.Value).Count();

                    if (staffBorrowingsToday + bookIds.Count > this.configRepository.MaxBooksStaffPerDay)
                    {
                        throw new InvalidOperationException(
                            $"Staff cannot distribute more than {this.configRepository.MaxBooksStaffPerDay} books per day.");
                    }
                }
            }

            var config = this.configRepository;

            var maxBooksPerRequest = reader.IsStaff ? config.MaxBooksPerRequest * 2 : config.MaxBooksPerRequest;

            if (bookIds.Count > maxBooksPerRequest)
            {
                Logger.WarnFormat("CreateBorrowings failed: exceeded max per request. count={0}, max={1}", bookIds.Count, maxBooksPerRequest);
                throw new InvalidOperationException($"Cannot borrow more than {maxBooksPerRequest} books at once.");
            }

            if (bookIds.Count >= 3)
            {
                var books = new List<Book>();
                foreach (var bookId in bookIds)
                {
                    var book = this.bookRepository.GetById(bookId);
                    if (book == null)
                    {
                        Logger.WarnFormat("CreateBorrowings failed: book not found. bookId={0}", bookId);
                        throw new InvalidOperationException($"Book {bookId} not found.");
                    }

                    books.Add(book);
                }

                var distinctDomains = books
                    .SelectMany(b => b.Domains ?? Enumerable.Empty<BookDomain>())
                    .Select(d => d.Id)
                    .Distinct()
                    .Count();

                if (distinctDomains < 2)
                {
                    Logger.Warn("CreateBorrowings failed: domain diversity rule violated.");
                    throw new InvalidOperationException("When borrowing 3 or more books, they must be from at least 2 different domains.");
                }
            }

            var maxBooksPerDay = reader.IsStaff ? int.MaxValue : config.MaxBooksPerDay;
            var todayBorrowings = this.borrowingRepository.GetBorrowingsByDateRange(borrowingDate.Date, borrowingDate) ?? Enumerable.Empty<Borrowing>();

            if (todayBorrowings.Count() + bookIds.Count > maxBooksPerDay)
            {
                Logger.WarnFormat("CreateBorrowings failed: daily limit exceeded. today={0}, requested={1}, max={2}", todayBorrowings.Count(), bookIds.Count, maxBooksPerDay);

                throw new InvalidOperationException($"Cannot borrow more than {maxBooksPerDay} books per day.");
            }

            var periodDays = config.BorrowingPeriodDays;
            var periodStart = borrowingDate.AddDays(-periodDays);
            var borrowingsInPeriod = this.borrowingRepository.GetBorrowingsByDateRange(periodStart, borrowingDate) ?? Enumerable.Empty<Borrowing>();

            var maxBooksInPeriod = reader.IsStaff ? config.MaxBooksPerPeriod * 2 : config.MaxBooksPerPeriod;

            if (borrowingsInPeriod.Count() + bookIds.Count > maxBooksInPeriod)
            {
                Logger.WarnFormat("CreateBorrowings failed: period limit exceeded. period={0}, requested={1}, max={2}", borrowingsInPeriod.Count(), bookIds.Count, maxBooksInPeriod);

                throw new InvalidOperationException($"Cannot borrow more than {maxBooksInPeriod} books in {periodDays} days.");
            }

            foreach (var bookId in bookIds)
            {
                if (!this.CanBorrowBook(readerId, bookId))
                {
                    Logger.WarnFormat("CreateBorrowings blocked by business rules. readerId={0}, bookId={1}", readerId, bookId);
                    throw new InvalidOperationException($"Cannot borrow book {bookId}. Business rules violated.");
                }

                var borrowing = new Borrowing
                {
                    ReaderId = readerId,
                    StaffId = staffId,
                    BookId = bookId,
                    BorrowingDate = borrowingDate,
                    DueDate = borrowingDate.AddDays(daysToBorrow),
                    IsActive = true,
                    InitialBorrowingDays = daysToBorrow,
                    TotalExtensionDays = 0,
                };

                try
                {
                    this.borrowingRepository.Add(borrowing);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("CreateBorrowings failed to persist borrowing. bookId={0}", bookId), ex);
                    throw new InvalidOperationException($"Failed to create borrowing record for book {bookId}: {ex.Message}", ex);
                }
            }

            Logger.InfoFormat("CreateBorrowings succeeded. readerId={0}, createdCount={1}", readerId, bookIds.Count);
        }
    }
}
