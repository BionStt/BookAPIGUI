﻿using BookApiProject.Dtos;
using BookGUI.Services;
using BookGUI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookGUI.Controllers
{
    public class HomeController : Controller
    {
        IBookRepositoryGUI _bookRepository;
        IAuthorRepositoryGUI _authorRepository;
        ICountryRepositoryGUI _countryRepository;
        ICategoryRepositoryGUI _categoryRepository;
        IReviewRepositoryGUI _reviewRepository;
        IReviewerRepositoryGUI _reviewerRepository;

        public HomeController(IBookRepositoryGUI bookRepository, IAuthorRepositoryGUI authorRepository,
                              ICountryRepositoryGUI countryRepository, ICategoryRepositoryGUI categoryRepository,
                              IReviewRepositoryGUI reviewRepository, IReviewerRepositoryGUI reviewerRepository)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _countryRepository = countryRepository;
            _categoryRepository = categoryRepository;
            _reviewRepository = reviewRepository;
            _reviewerRepository = reviewerRepository;
        }

        public IActionResult Index()
        {
            var books = _bookRepository.GetBooks();

            if(books.Count() <= 0)
            {
                ViewBag.Message = "There was a problem retrieving books from the database or no book exists";
            }

            var bookAuthorsCategoriesRatingViewModel = new List<BookAuthorsCategoriesRatingViewModel>();

            foreach(var book in books)
            {
                var authors = _authorRepository.GetAuthorsOfABook(book.Id).ToList();
                if (authors.Count() <= 0)
                    ModelState.AddModelError("", "Some kind of error getting authors");

                var categories = _categoryRepository.GetAllCategoriesOfABook(book.Id).ToList();
                if (categories.Count() <= 0)
                    ModelState.AddModelError("", "Some kind of error getting categories");

                var rating = _bookRepository.GetBookRating(book.Id);

                bookAuthorsCategoriesRatingViewModel.Add(new BookAuthorsCategoriesRatingViewModel
                {
                    Book = book,
                    Authors = authors,
                    Categories = categories,
                    Rating = rating
                });
            }

            return View(bookAuthorsCategoriesRatingViewModel);
        }

        public IActionResult GetBookById(int bookId)
        {
            var completeBookViewModel = new CompleteBookViewModel
            {
                AuthorsCountries = new Dictionary<AuthorDto, CountryDto>(),
                ReviewsReviewers = new Dictionary<ReviewDto, ReviewerDto>()
            };

            var book = _bookRepository.GetBookById(bookId);

            if (book == null)
            {
                ModelState.AddModelError("", "Some kind of error getting book");
                book = new BookDto();
            }

            var categories = _categoryRepository.GetAllCategoriesOfABook(bookId);
            if (categories.Count() <= 0)
            {
                ModelState.AddModelError("", "Some kind of error getting categories");
            }

            var rating = _bookRepository.GetBookRating(bookId);
            completeBookViewModel.Book = book;
            completeBookViewModel.Categories = categories;
            completeBookViewModel.Rating = rating;

            var authors = _authorRepository.GetAuthorsOfABook(bookId);
            if (authors.Count() <= 0)
            {
                ModelState.AddModelError("", "Some kind of error getting authors");
            }

            foreach (var author in authors)
            {
                var country = _countryRepository.GetCountryOfAnAuthor(author.Id);
                completeBookViewModel.AuthorsCountries.Add(author, country);
            }

            var reviews = _reviewRepository.GetReviewsOfABook(bookId);
            if (reviews.Count() <= 0)
            {
                ViewBag.ReviewsMessage = "There are no reviews yet";
            }

            foreach (var review in reviews)
            {
                var reviewer = _reviewerRepository.GetReviewerOfAReview(review.Id);
                completeBookViewModel.ReviewsReviewers.Add(review, reviewer);
            }

            if (!ModelState.IsValid)
            {
                ViewBag.BookMessage = "There was an error retrieving a complete book record";
            }

            return View(completeBookViewModel);
        }
    }
}