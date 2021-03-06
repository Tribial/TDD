﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Owin.Security.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using Moq;
using System.Web.Mvc;
using System.Web.Http;

namespace TDD.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
        }

        [TestMethod]
        public void ShouldLoginWithSuccess()
        {
            var user = new User { Username = "login", Email = "mail@mail.com", Password = "asd" };

            var loginModel = new LoginModel
            {
                Username = "login",
                Password = "asd"
            };
            var userRepository = new Mock<IRepository<User>>();
            userRepository.Setup(x => x.Exist(It.IsAny<Func<User, bool>>())).Returns(true);
            userRepository.Setup(x => x.GetBy(It.IsAny<Func<User, bool>>())).Returns(user);

            var userService = new UserService(userRepository.Object);

            var accountController = new AccountController(userService);


            var result = accountController.Login(loginModel);
            var okResult =(System.Web.Http.Results.OkNegotiatedContentResult<string>)result;
            var email = okResult.Content;

            Assert.AreEqual(user.Email, email);
        }

        public class User:Entity
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class LoginModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public interface IRepository<T> where T : Entity
        {
            bool Exist(Func<User,bool> function);
            T GetBy(Func<User, bool> function);
        }

        public interface IUserService
        {
            string Login(LoginModel loginModel);
        }

        public class UserService : IUserService
        {
            private readonly IRepository<User> _userRepository;
            public UserService (IRepository<User> userRepository)
	{
        _userRepository = userRepository;
	}
            public string Login(LoginModel loginModel)
            {
                var isUserExist = _userRepository.Exist(x => x.Username == loginModel.Username);

                if (!isUserExist)
                {
                    return null;
                }

                var user = _userRepository.GetBy(x => x.Username == loginModel.Username);

                if (user.Password == loginModel.Password)
                {
                    return user.Email;
                }

                return null;
            }
        }
        public class Entity
        {
            public long Id { get; set; }
        }

        public class AccountController:ApiController{
            private readonly IUserService _userService;

            public AccountController(IUserService userService)
            {
                _userService = userService;
            }

            public IHttpActionResult Login(LoginModel loginModel)
            {
                var userData = _userService.Login(loginModel);

                return Ok(userData);
            }
    }
        
    }
}
