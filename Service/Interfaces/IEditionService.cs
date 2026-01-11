// <copyright file="IEditionService.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace Service
{
    using System.Collections.Generic;
    using Domain.Models;

    /// <summary>
    /// Service interface for edition operations.
    /// </summary>
    public interface IEditionService
    {
        /// <summary>
        /// Gets all editions.
        /// </summary>
        /// <returns>Editions.</returns>
        IEnumerable<Edition> GetAllEditions();

        /// <summary>
        /// Gets an edition by ID.
        /// </summary>
        /// <param name="editionId">id.</param>
        /// <returns>Edition.</returns>
        Edition GetEditionById(int editionId);

        /// <summary>
        /// Gets editions by book ID.
        /// </summary>
        /// <param name="bookId">id.</param>
        /// <returns>Editions.</returns>
        IEnumerable<Edition> GetEditionsByBook(int bookId);

        /// <summary>
        /// Gets editions by publisher.
        /// </summary>
        /// <param name="publisher">publisher.</param>
        /// <returns>Editions.</returns>
        IEnumerable<Edition> GetEditionsByPublisher(string publisher);

        /// <summary>
        /// Creates a new edition.
        /// </summary>
        /// <param name="edition">edition.</param>
        void CreateEdition(Edition edition);

        /// <summary>
        /// Updates an edition.
        /// </summary>
        /// <param name="edition">edition.</param>
        void UpdateEdition(Edition edition);

        /// <summary>
        /// Deletes an edition.
        /// </summary>
        /// <param name="editionId">id.</param>
        void DeleteEdition(int editionId);
    }
}
