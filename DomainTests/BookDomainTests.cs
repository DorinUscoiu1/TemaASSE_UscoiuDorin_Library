// <copyright file="BookDomainTests.cs" company="Transilvania University of Brasov">
// Uscoiu Dorin Petrut
// </copyright>

namespace DomainTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Unit tests for the BookDomain model class.
    /// </summary>
    [TestClass]
    public class BookDomainTests
    {
        private BookDomain domain;

        /// <summary>
        /// Initializes the test by creating a new BookDomain instance before each test.
        /// </summary>
        [TestInitialize]
        public void TestInitialize()
        {
            this.domain = new BookDomain();
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookDomain_WithParentDomain_CreatesHierarchicalStructure()
        {
            var parentDomain = new BookDomain { Id = 1, Name = "Science" };
            var subDomain = new BookDomain { Id = 2, Name = "Physics", ParentDomainId = 1 };
            subDomain.ParentDomain = parentDomain;
            parentDomain.Subdomains.Add(subDomain);
            Assert.IsNotNull(subDomain.ParentDomain);
            Assert.AreEqual(parentDomain.Id, subDomain.ParentDomain.Id);
            Assert.AreEqual("Science", subDomain.ParentDomain.Name);
            Assert.IsTrue(parentDomain.Subdomains.Contains(subDomain));
            Assert.AreEqual(1, parentDomain.Subdomains.Count);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void IsAncestorOf_ChildToParent_ReturnsFalse()
        {
            var parent = new BookDomain { Id = 1, Name = "Science" };
            var child = new BookDomain { Id = 2, Name = "Computer Science", ParentDomain = parent };
            Assert.AreEqual(child.IsAncestorOf(parent), false);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookDomain_AddBooks_StoresBooksCorrectly()
        {
            this.domain.Id = 1;
            this.domain.Name = "Mathematics";
            var book1 = new Book { Id = 1, Title = "Algebra" };
            var book2 = new Book { Id = 2, Title = "Geometry" };
            this.domain.Books.Add(book1);
            this.domain.Books.Add(book2);
            Assert.AreEqual(2, this.domain.Books.Count);
            Assert.IsTrue(this.domain.Books.Contains(book1));
            Assert.IsTrue(this.domain.Books.Contains(book2));
            Assert.IsTrue(this.domain.Books.Any(b => b.Title == "Algebra"));
            Assert.IsTrue(this.domain.Books.Any(b => b.Title == "Geometry"));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookDomain_RootDomain_HasNoParent()
        {
            this.domain.Id = 1;
            this.domain.Name = "Root Domain";
            this.domain.ParentDomainId = null;
            Assert.IsNull(this.domain.ParentDomainId);
            Assert.IsNull(this.domain.ParentDomain);
            Assert.AreEqual("Root Domain", this.domain.Name);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookDomain_MultipleSubdomains_AreStoredCorrectly()
        {
            var parentDomain = new BookDomain { Id = 1, Name = "Science" };
            var subDomain1 = new BookDomain { Id = 2, Name = "Physics", ParentDomainId = 1 };
            var subDomain2 = new BookDomain { Id = 3, Name = "Chemistry", ParentDomainId = 1 };
            var subDomain3 = new BookDomain { Id = 4, Name = "Biology", ParentDomainId = 1 };
            parentDomain.Subdomains.Add(subDomain1);
            parentDomain.Subdomains.Add(subDomain2);
            parentDomain.Subdomains.Add(subDomain3);
            Assert.AreEqual(3, parentDomain.Subdomains.Count);
            Assert.IsTrue(parentDomain.Subdomains.All(s => s.ParentDomainId == 1));
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookDomain_GetAncestors_ReturnsCompleteChain()
        {
            var root = new BookDomain { Id = 1, Name = "Root" };
            var middle = new BookDomain { Id = 2, Name = "Middle", ParentDomain = root, ParentDomainId = 1 };
            var leaf = new BookDomain { Id = 3, Name = "Leaf", ParentDomain = middle, ParentDomainId = 2 };
            var ancestors = leaf.GetAncestors();
            Assert.AreEqual(3, ancestors.Count);
            Assert.AreEqual(leaf.Id, ancestors[0].Id);
            Assert.AreEqual(middle.Id, ancestors[1].Id);
            Assert.AreEqual(root.Id, ancestors[2].Id);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookDomain_IsAncestorOf_ReturnsTrueWhenAncestor()
        {
            var root = new BookDomain { Id = 1, Name = "Root" };
            var middle = new BookDomain { Id = 2, Name = "Middle", ParentDomain = root, ParentDomainId = 1 };
            var leaf = new BookDomain { Id = 3, Name = "Leaf", ParentDomain = middle, ParentDomainId = 2 };
            bool isAncestor = root.IsAncestorOf(leaf);
            Assert.IsTrue(isAncestor);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookDomain_IsAncestorOf_ReturnsFalseWhenNotAncestor()
        {
            var domain1 = new BookDomain { Id = 1, Name = "Domain1" };
            var domain2 = new BookDomain { Id = 2, Name = "Domain2" };
            bool isAncestor = domain1.IsAncestorOf(domain2);
            Assert.IsFalse(isAncestor);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookDomain_GetAncestors_RootReturnsOnlyItself()
        {
            var root = new BookDomain { Id = 1, Name = "Root" };
            var ancestors = root.GetAncestors();
            Assert.AreEqual(1, ancestors.Count);
            Assert.AreEqual(root.Id, ancestors[0].Id);
        }

        /// <summary>
        /// Test.
        /// </summary>
        [TestMethod]
        public void BookDomain_DefaultInitialization_HasCorrectDefaults()
        {
            var newDomain = new BookDomain();
            Assert.AreEqual(0, newDomain.Id);
            Assert.AreEqual(string.Empty, newDomain.Name);
            Assert.IsNull(newDomain.ParentDomainId);
            Assert.IsNull(newDomain.ParentDomain);
            Assert.IsNotNull(newDomain.Subdomains);
            Assert.IsNotNull(newDomain.Books);
            Assert.AreEqual(0, newDomain.Subdomains.Count);
            Assert.AreEqual(0, newDomain.Books.Count);
        }
    }
}
