using BlazorApp.Models;
using BlazorApp.Models.Account;
using BlazorApp.Helpers;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace BlazorApp.Services
{
    public interface IAccountService
    {
        User User { get; }
        Task Initialize();
        Task Login(Login model);
        Task Logout();
        Task Update(string id, EditUser model);
        Task Delete(string id);
        Task Register(AddUser model);
        Task<IList<User>> GetAll();
        Task<User> GetById(string id);
    }

    public class AccountService : IAccountService
    {
        private IHttpService _httpService;
        private NavigationManager _navigationManager;
        private ILocalStorageService _localStorageService;
        private string _userKey = "user";

        public User User { get; private set; }

        public AccountService(
            IHttpService httpService,
            NavigationManager navigationManager,
            ILocalStorageService localStorageService
        ) {
            _httpService = httpService;
            _navigationManager = navigationManager;
            _localStorageService = localStorageService;
        }

        public async Task Initialize()
        {
            var usersKey = "blazor-registration-login-example-users";
            var users = await _localStorageService.GetItem<List<UserRecord>>(usersKey) ?? null;
            if( users == null) {
                await Seed();
            }
            User = await _localStorageService.GetItem<User>(_userKey);
        }

        public async Task Login(Login model)
        {
            User = await _httpService.Post<User>("/users/authenticate", model);
            await _localStorageService.SetItem(_userKey, User);
        }

        //Seed local Storage with accounts
        public async Task Seed(){
            Models.Account.AddUser admin = new Models.Account.AddUser();
            admin.FirstName = "Admin";
            admin.LastName = "Test";
            admin.Username = "admin";
            admin.Password = "Password123";
            await Register(admin);
            Models.Account.AddUser me = new Models.Account.AddUser();
            me.FirstName = "Andrews";
            me.LastName = "P";
            me.Username = "andrews";
            me.Password = "Password123";
            await Register(me);
        }

        public async Task Update(string id, EditUser model)
        {
            await _httpService.Put($"/users/{id}", model);

            // update stored user if the logged in user updated their own record
            if (id == User.Id)
            {
                // update local storage
                User.FirstName = model.FirstName;
                User.LastName = model.LastName;
                User.Username = model.Username;
                await _localStorageService.SetItem(_userKey, User);
            }
        }

        public async Task Delete(string id)
        {
            await _httpService.Delete($"/users/{id}");

            // auto logout if the logged in user deleted their own record
            if (id == User.Id)
                await Logout();
        }

        public async Task Logout()
        {
            User = null;
            await _localStorageService.RemoveItem(_userKey);
            _navigationManager.NavigateTo("account/login");
        }

        public async Task Register(AddUser model)
        {
            await _httpService.Post("/users/register", model);
        }

        public async Task<IList<User>> GetAll()
        {
            return await _httpService.Get<IList<User>>("/users");
        }

        public async Task<User> GetById(string id)
        {
            return await _httpService.Get<User>($"/users/{id}");
        }
    }
}