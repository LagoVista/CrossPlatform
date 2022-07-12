using BugLog.Managers;
using BugLog.Models;
using LagoVista.Client.Core;
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.ProjectManagement.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BugLog.ViewModels
{
    public class RepoSupportViewModel : AppViewModelBase
    {
        WorkTaskSummary _wts;
        RepoManager _repoManager;
        private readonly LagoVista.Client.Core.IConsoleWriter _consoleWriter;
        IProcessRunner _processRunner;

        public RepoSupportViewModel(WorkTaskSummary wts, RepoManager repoManager, IDispatcherServices dispatcher)
        {
            _wts = wts ?? throw new ArgumentNullException(nameof(wts));
            _repoManager = repoManager ?? throw new ArgumentNullException(nameof(repoManager));
            _consoleWriter = new ConsoleWriter();
            _consoleWriter.Init(ConsoleLogOutput, dispatcher);

            NewRepoCommand = RelayCommand.Create(NewRepo);
            SaveRepoCommand = RelayCommand.Create(SaveRepo);
            CancelRepoCommand = RelayCommand.Create(CancelRepo);
            GitCommand = RelayCommand<string>.Create((cmd) => RunGitCommand(cmd));
            _processRunner = SLWIOC.Get<IProcessRunner>();
            _processRunner.Init(_consoleWriter);
        }

        async void RunGitCommand(string cmd)
        {
            if (Repo != null)
            {
                await _processRunner.RunProcess(Repo.Folder, "git.exe", cmd, String.Empty);
            }
        }

        public void NewRepo()
        {
            Repo = new Repo()
            {
                ProjectId = _wts.ProjectId
            };
        }

        public async override Task InitAsync()
        {
            Repos = new ObservableCollection<Repo>(await _repoManager.GetReposForProjectAsync(_wts.ProjectId));
        }


        public async void SaveRepo()
        {
            await _repoManager.UpdateRepoAsync(Repo);
            Repo = null;
        }
        public void CancelRepo()
        {
            Repo = null;
        }

        ObservableCollection<Repo> _repos;
        public ObservableCollection<Repo> Repos
        {
            get => _repos;
            set => Set(ref _repos, value);
        }

        private Repo _repo;
        public Repo Repo
        {
            get => _repo;
            set => Set(ref _repo, value);
        }

        public RelayCommand NewRepoCommand { get; }
        public RelayCommand SaveRepoCommand { get; }
        public RelayCommand CancelRepoCommand { get; }

        public RelayCommand<string> GitCommand { get; }

        public ObservableCollection<ConsoleOutput> ConsoleLogOutput { get; } = new ObservableCollection<ConsoleOutput>();
    }
}
