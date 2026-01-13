// <copyright file="BookDomainServiceTests.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace ServiceTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Data;
    using Data.Repositories;
    using Domain.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Rhino.Mocks;
    using Service;

    /// <summary>
    /// Unit tests for BookDomainService with Rhino Mocks and Integration Tests.
    /// </summary>
    [TestClass]
    public class BookDomainServiceTests
    {
        private IBookDomain mockDomainRepository;
        private LibraryConfiguration mockConfigRepository;
        private BookDomainService domainService;

        /// <summary>
        /// Initializes test fixtures before each test with mocked dependencies.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.mockDomainRepository = MockRepository.GenerateStub<IBookDomain>();
            this.mockConfigRepository = new LibraryConfiguration();

            this.domainService = new BookDomainService(
                this.mockDomainRepository);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetAllDomains_WhenCalled_ReturnsAllDomains()
        {
            // Arrange
            var domains = new List<BookDomain>
            {
                new BookDomain { Id = 1, Name = "Science" },
                new BookDomain { Id = 2, Name = "Technology" },
                new BookDomain { Id = 3, Name = "Mathematics" },
            };

            this.mockDomainRepository.Stub(x => x.GetAll()).Return(domains);
            var result = this.domainService.GetAllDomains();
            Assert.AreEqual(3, result.Count());
            Assert.IsTrue(result.Any(d => d.Name == "Science"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetDomainById_WithValidId_ReturnsCorrectDomain()
        {
            var domain = new BookDomain { Id = 1, Name = "Science", ParentDomainId = null };
            this.mockDomainRepository.Stub(x => x.GetById(1)).Return(domain);
            var result = this.domainService.GetDomainById(1);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("Science", result.Name);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetRootDomains_WhenCalled_ReturnsOnlyRootDomains()
        {
            var rootDomains = new List<BookDomain>
            {
                new BookDomain { Id = 1, Name = "Science", ParentDomainId = null },
                new BookDomain { Id = 2, Name = "Art", ParentDomainId = null },
            };

            this.mockDomainRepository.Stub(x => x.GetRootDomains()).Return(rootDomains);
            var result = this.domainService.GetRootDomains();
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(d => d.ParentDomainId == null));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetSubdomains_WithValidParentId_ReturnsSubdomains()
        {
            var subdomains = new List<BookDomain>
            {
                new BookDomain { Id = 2, Name = "Physics", ParentDomainId = 1 },
                new BookDomain { Id = 3, Name = "Chemistry", ParentDomainId = 1 },
            };
            this.mockDomainRepository.Stub(x => x.GetSubdomains(1)).Return(subdomains);
            var result = this.domainService.GetSubdomains(1);
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.All(d => d.ParentDomainId == 1));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateDomain_WithNullName()
        {
            var domain = new BookDomain { Name = null };
            this.domainService.CreateDomain(domain);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateDomain_WithEmptyName()
        {
            var domain = new BookDomain { Name = string.Empty };
            this.domainService.CreateDomain(domain);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CreateDomain_WithValidData()
        {
            var domain = new BookDomain { Name = "Science" };
            this.domainService.CreateDomain(domain);
            this.mockDomainRepository.AssertWasCalled(x => x.Add(Arg<BookDomain>.Is.Anything));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetAncestorDomains_WithMultipleLevels_ReturnsAllAncestors()
        {
            var grandparent = new BookDomain { Id = 1, Name = "Grandparent", ParentDomainId = null };
            var parent = new BookDomain { Id = 2, Name = "Parent", ParentDomainId = 1 };
            var child = new BookDomain { Id = 3, Name = "Child", ParentDomainId = 2 };

            this.mockDomainRepository.Stub(x => x.GetById(3)).Return(child);
            this.mockDomainRepository.Stub(x => x.GetById(2)).Return(parent);
            this.mockDomainRepository.Stub(x => x.GetById(1)).Return(grandparent);
            var result = this.domainService.GetAncestorDomains(3).ToList();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Parent", result[0].Name);
            Assert.AreEqual("Grandparent", result[1].Name);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetDescendantDomains_WithMultipleLevels()
        {
            var child1 = new BookDomain { Id = 2, Name = "Child1", ParentDomainId = 1 };
            var child2 = new BookDomain { Id = 3, Name = "Child2", ParentDomainId = 1 };
            var grandchild = new BookDomain { Id = 4, Name = "Grandchild", ParentDomainId = 2 };

            this.mockDomainRepository.Stub(x => x.GetSubdomains(1)).Return(new List<BookDomain> { child1, child2 });
            this.mockDomainRepository.Stub(x => x.GetSubdomains(2)).Return(new List<BookDomain> { grandchild });
            this.mockDomainRepository.Stub(x => x.GetSubdomains(3)).Return(new List<BookDomain>());
            this.mockDomainRepository.Stub(x => x.GetSubdomains(4)).Return(new List<BookDomain>());

            var result = this.domainService.GetDescendantDomains(1).ToList();
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Any(d => d.Name == "Child1"));
            Assert.IsTrue(result.Any(d => d.Name == "Child2"));
            Assert.IsTrue(result.Any(d => d.Name == "Grandchild"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookDomainService_GetSubdomains()
        {
            using (var context = new LibraryDbContext())
            {
                var domainRepo = new BookDomainDataService(context);
                var service = new BookDomainService(domainRepo);

                var parent = new BookDomain { Name = "Parent_" + Guid.NewGuid().ToString().Substring(0, 5) };
                context.Domains.Add(parent);
                context.SaveChanges();

                var child1 = new BookDomain
                {
                    Name = "Child1_" + Guid.NewGuid().ToString().Substring(0, 5),
                    ParentDomainId = parent.Id,
                };
                var child2 = new BookDomain
                {
                    Name = "Child2_" + Guid.NewGuid().ToString().Substring(0, 5),
                    ParentDomainId = parent.Id,
                };
                context.Domains.Add(child1);
                context.Domains.Add(child2);
                context.SaveChanges();

                try
                {
                    var result = service.GetSubdomains(parent.Id);
                    Assert.AreEqual(2, result.Count());
                }
                finally
                {
                    context.Domains.Remove(child1);
                    context.Domains.Remove(child2);
                    context.Domains.Remove(parent);
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookDomainService_GetRootDomains()
        {
            using (var context = new LibraryDbContext())
            {
                var domainRepo = new BookDomainDataService(context);
                var service = new BookDomainService(domainRepo);

                var root = new BookDomain { Name = "Root_" + Guid.NewGuid().ToString().Substring(0, 5) };
                context.Domains.Add(root);
                context.SaveChanges();

                var child = new BookDomain
                {
                    Name = "Child_" + Guid.NewGuid().ToString().Substring(0, 5),
                    ParentDomainId = root.Id,
                };
                context.Domains.Add(child);
                context.SaveChanges();

                try
                {
                    var result = service.GetRootDomains();
                    Assert.IsTrue(result.Any(d => d.Id == root.Id));
                    Assert.IsFalse(result.Any(d => d.Id == child.Id));
                }
                finally
                {
                    context.Domains.Remove(child);
                    context.Domains.Remove(root);
                    context.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UpdateDomain_WithNullDomain()
        {
            this.domainService.UpdateDomain(null);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UpdateDomain_WithWhitespaceName()
        {
            var domain = new BookDomain { Id = 1, Name = "   " };
            this.domainService.UpdateDomain(domain);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void UpdateDomain_WithSelfAsParent()
        {
            var domain = new BookDomain { Id = 1, Name = "Science", ParentDomainId = 1 };
            this.domainService.UpdateDomain(domain);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void UpdateDomain_WithValidData()
        {
            var domain = new BookDomain { Id = 1, Name = "Science", ParentDomainId = null };
            this.domainService.UpdateDomain(domain);
            this.mockDomainRepository.AssertWasCalled(x => x.Update(Arg<BookDomain>.Is.Same(domain)));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CreateDomain_IntoDatabase()
        {
            using (var context = new LibraryDbContext())
            {
                var realRepository = new BookDomainDataService(context);
                var realService = new BookDomainService(realRepository);

                var domain = new BookDomain
                {
                    Name = "Integration Test Domain",
                    ParentDomainId = null,
                };
                realService.CreateDomain(domain);
                var retrievedDomain = context.Domains.FirstOrDefault(d => d.Name == "Integration Test Domain");

                Assert.IsNotNull(retrievedDomain, "Domain should exist in database");
                Assert.AreEqual("Integration Test Domain", retrievedDomain.Name);
                Assert.IsNull(retrievedDomain.ParentDomainId);
                Assert.IsTrue(retrievedDomain.Id > 0);

                context.Domains.Remove(retrievedDomain);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void CreateDomain_WithParentDomain_StoresParentChildRelationship()
        {
            using (var context = new LibraryDbContext())
            {
                var realRepository = new BookDomainDataService(context);
                var realService = new BookDomainService(realRepository);
                var parentDomain = new BookDomain { Name = "Parent Science Domain", ParentDomainId = null };
                realService.CreateDomain(parentDomain);
                var createdParent = context.Domains.FirstOrDefault(d => d.Name == "Parent Science Domain");
                Assert.IsNotNull(createdParent);
                var childDomain = new BookDomain
                {
                    Name = "Child Physics Domain",
                    ParentDomainId = createdParent.Id,
                };
                realService.CreateDomain(childDomain);
                var retrievedChild = context.Domains.FirstOrDefault(d => d.Name == "Child Physics Domain");

                Assert.IsNotNull(retrievedChild);
                Assert.AreEqual(createdParent.Id, retrievedChild.ParentDomainId);

                context.Domains.Remove(retrievedChild);
                context.Domains.Remove(createdParent);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void GetDomainById_FromDatabase()
        {
            using (var context = new LibraryDbContext())
            {
                var realRepository = new BookDomainDataService(context);
                var realService = new BookDomainService(realRepository);

                var domain = new BookDomain { Name = "GetById Test Domain" };
                realService.CreateDomain(domain);

                var insertedDomain = context.Domains
                    .FirstOrDefault(d => d.Name == "GetById Test Domain");
                Assert.IsNotNull(insertedDomain);
                int domainId = insertedDomain.Id;
                var retrievedDomain = realService.GetDomainById(domainId);
                Assert.IsNotNull(retrievedDomain);
                Assert.AreEqual(domainId, retrievedDomain.Id);
                Assert.AreEqual("GetById Test Domain", retrievedDomain.Name);
                context.Domains.Remove(insertedDomain);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void UpdateDomain_InDatabase()
        {
            using (var context = new LibraryDbContext())
            {
                var realRepository = new BookDomainDataService(context);
                var realService = new BookDomainService(realRepository);

                var domain = new BookDomain { Name = "Original Domain Name" };
                realService.CreateDomain(domain);

                var insertedDomain = context.Domains.FirstOrDefault(d => d.Name == "Original Domain Name");
                Assert.IsNotNull(insertedDomain);
                insertedDomain.Name = "Updated Domain Name";
                realService.UpdateDomain(insertedDomain);
                var updatedDomain = context.Domains.Find(insertedDomain.Id);
                Assert.AreEqual("Updated Domain Name", updatedDomain.Name);
                context.Domains.Remove(updatedDomain);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void DeleteDomain_FromDatabase()
        {
            using (var context = new LibraryDbContext())
            {
                var realRepository = new BookDomainDataService(context);
                var realService = new BookDomainService(realRepository);

                var domain = new BookDomain { Name = "Delete Test Domain" };
                realService.CreateDomain(domain);

                var insertedDomain = context.Domains.FirstOrDefault(d => d.Name == "Delete Test Domain");
                Assert.IsNotNull(insertedDomain);
                int domainId = insertedDomain.Id;
                realService.DeleteDomain(domainId);
                var deletedDomain = context.Domains.Find(domainId);
                Assert.IsNull(deletedDomain, "Domain should be deleted from database");
            }
        }
    }
}
