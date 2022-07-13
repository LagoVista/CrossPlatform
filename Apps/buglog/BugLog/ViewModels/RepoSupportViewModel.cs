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
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
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
            CancelRepoCommand = RelayCommand.Create(() => EditRepo = null);
            GitCommand = RelayCommand<string>.Create((cmd) => RunGitCommand(cmd));
            _processRunner = SLWIOC.Get<IProcessRunner>();
            _processRunner.Init(_consoleWriter);
        }

        public string TaskBranchName
        {
            get => $"sl/{Task.TaskCode.ToLower().Replace("-", "_")}";
        }

        async void RunGitCommand(string cmd)
        {
            if (CurrentRepo != null)
            {
                switch(cmd)
                {
                    case "create":
                        cmd = $"checkout -b {TaskBranchName}";
                        await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                        break;

                    case "rebasetest":
                        cmd = $"checkout {CurrentRepo.TestBranch}";
                        await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);

                        cmd = $"pull";
                        await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                        break;

                    case "commit":
                        cmd = $"add .";
                        await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);

                        cmd = $"commit -m \"{CommitMessage}";
                        await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                        break;

                    case "squash":
                        cmd = $"merge-base {CurrentRepo.DevBranch} {TaskBranchName}";
                        var response = await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                        await System.Threading.Tasks.Task.Delay(500);

                        cmd = $"reset --soft  {response.Trim()}";
                        await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                        await System.Threading.Tasks.Task.Delay(500);

                        cmd = $"add .";
                        await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                        await System.Threading.Tasks.Task.Delay(500);

                        cmd = $"commit -m  \"{Task.Name.ToLower()}\r\n#{Task.ExternalTaskCode}";
                        await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                        break;

                    case "rebasedev":
                        cmd = $"checkout {CurrentRepo.DevBranch}";
                        await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);

                        cmd = $"pull";
                        await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                        break;
                        

                    case "branch":
                        response = await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                        var onCurrentBranch = false;
                        var currentBranchClean = false;
                        var hasBranchButNotCurrent = false;

                        var currentRegEx = new Regex(@"\*\s(?'current'[a-z\/_0-9]+)");
                        var currentMatch = currentRegEx.Match(response);
                        if (currentMatch.Success)
                        {
                            var currentBranchName = currentMatch.Groups["current"].Value;
                            if (currentBranchName == TaskBranchName)
                            {
                                Debug.WriteLine("On current branch.");
                                onCurrentBranch = true;
                            }
                        }
                                                
                        if(!onCurrentBranch)
                        {
                            var lines = response.Split('\n');
                            foreach (var line in lines)
                            {
                                if(line.Trim() == TaskBranchName)
                                {
                                    hasBranchButNotCurrent = true;
                                }
                            }                         
                        }

                        if(!onCurrentBranch && hasBranchButNotCurrent)
                        {
                            response = await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", "status", String.Empty);
                            Debug.WriteLine(response);
                            if(response.Contains("working tree clean"))
                            {
                                currentBranchClean = true;
                            }
                        }
                        
                        if(!onCurrentBranch && currentBranchClean && hasBranchButNotCurrent)
                        {
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", $"checkout {TaskBranchName}", String.Empty);
                        }

                        break;

                    default:
                        response = await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                        Debug.WriteLine(response);
                        break;
                }

                
            }
        }

        public void NewRepo()
        {
            EditRepo = new Repo()
            {
                ProjectId = _wts.ProjectId
            };
        }

        public async override Task InitAsync()
        {
            Repos = new ObservableCollection<Repo>(await _repoManager.GetReposForProjectAsync(_wts.ProjectId));
            CurrentRepo = Repos.FirstOrDefault();
        }

        public async void SaveRepo()
        {
            await _repoManager.UpdateRepoAsync(EditRepo);
            EditRepo = null;
        }

        ObservableCollection<Repo> _repos;
        public ObservableCollection<Repo> Repos
        {
            get => _repos;
            set => Set(ref _repos, value);
        }

        private Repo _repo;
        public Repo EditRepo
        {
            get => _repo;
            set => Set(ref _repo, value);
        }

        private Repo _currentRepo;
        public Repo CurrentRepo
        {
            get => _currentRepo;
            set => Set(ref _currentRepo, value);
        }

        private string _commitMessage;
        public string CommitMessage
        {
            get => _commitMessage;
            set => Set(ref _commitMessage, value);
        }

        public WorkTaskSummary Task { get => _wts; }

        public RelayCommand NewRepoCommand { get; }
        public RelayCommand SaveRepoCommand { get; }
        public RelayCommand CancelRepoCommand { get; }
        public RelayCommand<string> GitCommand { get; }

        public ObservableCollection<ConsoleOutput> ConsoleLogOutput { get; } = new ObservableCollection<ConsoleOutput>();
    }
}
