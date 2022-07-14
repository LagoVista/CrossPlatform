using BugLog.Managers;
using BugLog.Models;
using LagoVista.Client.Core;
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
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
            RunGitCommandCommand = RelayCommand.Create((cmd) => RunGitCommand(GitCommandText));
            SaveRepoCommand = RelayCommand.Create(SaveRepo);
            CancelRepoCommand = RelayCommand.Create(() => EditRepo = null);
            CreatePRCommand = RelayCommand.Create(CreatePR);
            GitCommand = RelayCommand<string>.Create((cmd) => RunGitCommand(cmd));
            _processRunner = SLWIOC.Get<IProcessRunner>();
            _processRunner.Init(_consoleWriter);
        }

        public string TaskBranchName
        {
            get => $"sl/{Task.TaskCode.ToLower()}";
        }

        async void RunGitCommand(string cmd)
        {
            StepOutput.Clear();

            if (CurrentRepo != null)
            {
                switch (cmd)
                {
                    case "create":
                        if (!await IsCurrentBranchClean())
                        {
                            await Popups.ShowAsync("Current branch has uncommitted changes, please commit or stash your changes.");
                            StepOutput.Add("Current branch has uncommitted changes, please commit or stash your changes.");
                        }
                        else
                        {
                            StepOutput.Add("Checking Out Dev Branch");
                            cmd = $"checkout {CurrentRepo.DevBranch}";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);
                            StepOutput[StepOutput.Count - 1] += " - OK";

                            StepOutput.Add("Pulling Dev Branch");
                            cmd = $"pull";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);
                            StepOutput[StepOutput.Count - 1] += " - OK";

                            StepOutput.Add($"Creating New Branch - {TaskBranchName}");
                            cmd = $"checkout -b {TaskBranchName}";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);
                            StepOutput[StepOutput.Count - 1] += " - OK";

                            IsCurrentBranchConfigured = true;

                        }
                        break;

                    case "rebasetest":
                        if (!await IsCurrentBranchClean())
                        {
                            await Popups.ShowAsync("Current branch has uncommitted changes, please commit or stash your changes.");
                            StepOutput.Add("Current branch has uncommitted changes, please commit or stash your changes.");
                        }
                        else
                        {
                            cmd = $"checkout {CurrentRepo.TestBranch}";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);

                            cmd = $"pull";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);

                            cmd = $"checkout {TaskBranchName}";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);

                            cmd = $"rebase {CurrentRepo.TestBranch}";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);
                        }
                        break;
                    case "createonserver":
                        if (!await IsCurrentBranchClean())
                        {
                            await Popups.ShowAsync("Current branch has uncommitted changes, please commit or stash your changes.");
                            StepOutput.Add("Current branch has uncommitted changes, please commit or stash your changes.");
                        }
                        else
                        {
                            StepOutput.Add($"Creating {TaskBranchName} branch on server.");
                            cmd = $"push --set-upstream origin {TaskBranchName}";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);
                            StepOutput[StepOutput.Count - 1] += " - OK";
                        }

                            break;

                    case "rebasedev":
                        if (!await IsCurrentBranchClean())
                        {
                            await Popups.ShowAsync("Current branch has uncommitted changes, please commit or stash your changes.");
                            StepOutput.Add("Current branch has uncommitted changes, please commit or stash your changes.");
                        }
                        else
                        {
                            StepOutput.Add($"Switching to {CurrentRepo.DevBranch} branch.");
                            cmd = $"checkout {CurrentRepo.DevBranch}";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);
                            StepOutput[StepOutput.Count - 1] += " - OK";

                            StepOutput.Add($"Pulling latest from {CurrentRepo.DevBranch} branch.");
                            cmd = $"pull";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);
                            StepOutput[StepOutput.Count - 1] += " - OK";

                            StepOutput.Add($"Switching back to {TaskBranchName} branch.");
                            cmd = $"checkout {TaskBranchName}";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);
                            StepOutput[StepOutput.Count - 1] += " - OK";

                            StepOutput.Add($"Rebase from {CurrentRepo.DevBranch} branch onto {TaskBranchName}.");
                            cmd = $"rebase {CurrentRepo.DevBranch}";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);
                        }
                        break;

                    case "commit":
                        if (await IsCurrentBranchClean())
                        {
                            StepOutput.Add("It does not appear as if you have any uncomitted changes.");
                        }
                        else
                        {
                            StepOutput.Add("Adding Files.");
                            cmd = $"add .";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);
                            StepOutput[StepOutput.Count - 1] += " - OK";

                            StepOutput.Add("Committing Files.");
                            cmd = $"commit -m \"{CommitMessage}";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);
                            StepOutput[StepOutput.Count - 1] += " - OK";
                            StepOutput.Add("Done.");
                        }
                        break;

                    case "squash":
                        if (!await IsCurrentBranchClean())
                        {
                            StepOutput.Add("It does not appear as if you have any uncomitted changes.");
                        }
                        else
                        {
                            StepOutput.Add($"Identify initial branch");
                            cmd = $"merge-base {CurrentRepo.DevBranch} {TaskBranchName}";
                            var response = await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);
                            StepOutput[StepOutput.Count - 1] += " - OK";

                            StepOutput.Add($"Soft resetting current branch");
                            cmd = $"reset --soft  {response.Trim()}";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);
                            StepOutput[StepOutput.Count - 1] += " - OK";

                            StepOutput.Add($"Adding changes");
                            cmd = $"add .";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            await System.Threading.Tasks.Task.Delay(500);
                            StepOutput[StepOutput.Count - 1] += " - OK";

                            StepOutput.Add($"Committing changes");
                            cmd = $"commit -m  \"{Task.Name.ToLower()}\r\n#{Task.ExternalTaskCode}";
                            await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            StepOutput[StepOutput.Count - 1] += " - OK";
                        }
                        break;

                    default:
                        {
                            var response = await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", cmd, String.Empty);
                            Debug.WriteLine(response);
                        }
                        break;
                }
            }
        }

        public async Task<bool> IsCurrentBranchClean()
        {
            var response = await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", "status", String.Empty);
            Debug.WriteLine(response);
            return (response.Contains("working tree clean"));
        }

        public async Task CheckForCurrentBranch()
        {
            if (CurrentRepo != null)
            {
                var response = await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", "branch", String.Empty);
                var onCurrentBranch = false;
                var currentBranchClean = false;
                var hasBranchButNotCurrent = false;

                var currentRegEx = new Regex(@"\*\s(?'current'[a-z\/_\-0-9]+)");
                var currentMatch = currentRegEx.Match(response);
                if (currentMatch.Success)
                {
                    var currentBranchName = currentMatch.Groups["current"].Value;
                    if (currentBranchName == TaskBranchName)
                    {
                        Debug.WriteLine("On current branch.");
                        IsCurrentBranchConfigured = true;
                        return;
                    }
                }

                if (!onCurrentBranch)
                {
                    var lines = response.Split('\n');
                    foreach (var line in lines)
                    {
                        if (line.Trim() == TaskBranchName)
                        {
                            hasBranchButNotCurrent = true;
                        }
                    }
                }

                if (!onCurrentBranch && hasBranchButNotCurrent)
                {
                    currentBranchClean = await IsCurrentBranchClean();
                }

                if (!onCurrentBranch && currentBranchClean && hasBranchButNotCurrent)
                {
                    await _processRunner.RunProcess(CurrentRepo.Folder, "git.exe", $"checkout {TaskBranchName}", String.Empty);
                    IsCurrentBranchConfigured = true;
                }
                else if (!onCurrentBranch)
                {
                    await Popups.ShowAsync($"Branch for {Task.TaskCode} not found\r\nPlease make sure you have selected the correct repository, and try again.\r\nOr if you need to create your branch, make sure you are on the correct repository, have no uncommitted changes and Create a branch.");
                }
            }
        }

        public void CreatePR()
        {
            var url = $"https://dev.azure.com/accustore/IT-Development/_git/GSP.WebClient/pullrequestcreate?sourceRef={TaskBranchName}";
            Services.Network.OpenURI(new System.Uri(url));
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
            await CheckForCurrentBranch();
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

        private string _gitCommandText;
        public string GitCommandText
        {
            get => _gitCommandText;
            set => Set(ref _gitCommandText, value);
        }
        

        private bool _isCurrentBranchConfigured;
        public bool IsCurrentBranchConfigured
        {
            get => _isCurrentBranchConfigured;
            set => Set(ref _isCurrentBranchConfigured, value);
        }

        public WorkTaskSummary Task { get => _wts; }

        public RelayCommand NewRepoCommand { get; }

        public RelayCommand RunGitCommandCommand { get; }
        public RelayCommand SaveRepoCommand { get; }
        public RelayCommand CancelRepoCommand { get; }
        public RelayCommand<string> GitCommand { get; }

        public RelayCommand CreatePRCommand { get; }

        public ObservableCollection<ConsoleOutput> ConsoleLogOutput { get; } = new ObservableCollection<ConsoleOutput>();

        public ObservableCollection<string> StepOutput { get; } = new ObservableCollection<string>();
    }
}
