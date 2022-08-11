using BugLog.Models;
using LagoVista.Core;
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BugLog.Managers
{
    public class RepoManager
    {
        private readonly IStorageService _storageService;
        private List<Repo> _repos;


        public RepoManager(IStorageService storageService)
        {
            this._storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        }

        public async Task<List<Repo>> GetReposForProjectAsync(string id)
        {
            _repos = await _storageService.GetAsync<List<Repo>>("repos.json");
            if (_repos == null)
                _repos = new List<Repo>();
            return _repos.Where(repo => repo.ProjectId == id).ToList();
        }

        public async Task UpdateRepoAsync(Repo repo)
        {
            if (_repos == null)
                _repos = await _storageService.GetAsync<List<Repo>>("repos.json");

            if (_repos == null)
                _repos = new List<Repo>();

            if (String.IsNullOrEmpty(repo.Id))
            {
                repo.Id = Guid.NewGuid().ToId();
                _repos.Add(repo);
            }

            await _storageService.StoreAsync(_repos, "repos.json");
        }
    }
}
