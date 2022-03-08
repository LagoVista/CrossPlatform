using BugLog.Models;
using LagoVista.Client.Core.Exceptions;
using LagoVista.Client.Core.Resources;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.Other;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.ProjectManagement.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BugLog.ViewModels
{
    public class MainViewModel : AppViewModelBase
    {
        public MainViewModel()
        {
            MenuItems = new List<MenuItem>()
            {
                new MenuItem()
                {
                    Command = new RelayCommand(() => ViewModelNavigation.NavigateAsync<AboutViewModel>(this)),
                    Name = "About",
                    FontIconKey = "fa-info"
                },
                new MenuItem()
                {
                    Command = new RelayCommand(() => Logout()),
                    Name = ClientResources.Common_Logout,
                    FontIconKey = "fa-sign-out"
                }
            };

            CloseStatusUpdateCommand = RelayCommand.Create(CloseStatusWindow);
            AddTaskCommand = RelayCommand.Create(AddTask);
            AddTimeCommand = RelayCommand<WorkTaskSummary>.Create(AddTime);
            RefreshTasksCommand = RelayCommand.Create(RefreshTasks);
            OpenFullTaskCommand = RelayCommand<WorkTaskSummary>.Create(OpenFullTask);
            AddRelatedTaskCommand = RelayCommand<WorkTaskSummary>.Create(AddRelatedTask);
            AddTimeCommand = RelayCommand<WorkTaskSummary>.Create(AddTime);
            AddExpectedOutcomeCommand = RelayCommand.Create(AddExpectedOutcome);
            SaveNewTaskCommand = RelayCommand.Create(SaveNewTask);
            CancelNewTimeCommand = RelayCommand.Create(() => TimeEntryTask = null);
            SaveNewTimeCommand = RelayCommand.Create(SaveNewTime);
            CancelNewTaskCommand = RelayCommand.Create(() => NewWorkTask = null);
            TimeEntryNextDateCommand = RelayCommand.Create(() => TimeEntryDate = TimeEntryDate.AddDays(1));
            TimeEntryPrevDateCommand = RelayCommand.Create(() => TimeEntryDate = TimeEntryDate.AddDays(-1));
        }

        async void CloseStatusWindow()
        {
            var statusUpdate = new WorkTaskAssignmentStatusUpdate();
            if (!String.IsNullOrEmpty(StatusUpdateNotes))
            {
                statusUpdate.Notes = StatusUpdateNotes;
            }

            if (SelectedTransition.Key != "-1")
            {
                statusUpdate.Status = SelectedTransition.Key;
            }

            if (SelectedExternalTransition.Key != "-1")
            {
                statusUpdate.ExternalStatus = SelectedExternalTransition.Key;
            }

            if (!String.IsNullOrEmpty(statusUpdate.Notes) ||
               !string.IsNullOrEmpty(statusUpdate.Status) ||
               !string.IsNullOrEmpty(statusUpdate.ExternalStatus))
            {
                await PerformNetworkOperation(async () =>
                {
                    var result = await this.RestClient.PutAsync($"/api/pm/task/{TaskSummary.Id}/boardupdate", statusUpdate);
                    if (result.Successful)
                    {
                        if (SelectedTransition.Key != "-1")
                        {
                            TaskSummary.Status = SelectedTransition.Name;
                            TaskSummary.StatusId = SelectedTransition.Key;
                        }

                        if (SelectedExternalTransition.Key != "-1")
                        {
                            TaskSummary.ExternalStatus = SelectedExternalTransition.Name;
                            TaskSummary.ExternalStatusId = SelectedExternalTransition.Key;
                        }

                        TaskSummary = null;
                        StatusUpdateNotes = null;
                        this.FilterTasks();
                    }
                    return result;
                });
            }
            else
            {
                this.TaskSummary = null;
            }
        }

        void AddTime(WorkTaskSummary taskSummary)
        {
            TimeEntryTask = taskSummary;
            TimeEntryHours = "1";
            TimeEntryNotes = String.Empty;
            TimeEntryDate = DateTime.Now.Date;
        }

        void AddTask()
        {
            NewWorkTask = new NewTask();

            if (TaskSummary != null)
            {
                NewWorkTask.Project = Projects.Single(prj => prj.Id == TaskSummary.ProjectId);
                TaskSummary = null;
            }
        }

        void AddExpectedOutcome()
        {
            NewWorkTask.ExpectedOutcomes.Add(new ExpectedOutcome()
            {

            });
        }

        void AddRelatedTask(WorkTaskSummary taskSummary)
        {
            var newTask = new NewTask();
            newTask.Project = Projects.Single(prj => prj.Id == taskSummary.ProjectId);
            newTask.Parent = EntityHeader.Create(taskSummary.Id, taskSummary.Name);
            newTask.TaskType = WorkTaskTypesForNewTask.SingleOrDefault(tsk => tsk.Key == "defect");
            NewWorkTask = newTask;
        }

        void SaveNewTime()
        {
            PerformNetworkOperation(async () =>
            {
                if (double.TryParse(TimeEntryHours, out double hours))
                {
                    var timeEntry = new TimeEntry()
                    {
                        Project = EntityHeader.Create(TimeEntryTask.ProjectId, TimeEntryTask.ProjectName),
                        WorkTask = EntityHeader.Create(TimeEntryTask.Id, TimeEntryTask.Name),
                        Date = TimeEntryDate.ToString(@"yyyy/MM/dd"),
                        Hours = hours,
                        Notes = TimeEntryNotes,
                        UserId = AuthManager.User.Id
                    };

                    var result = await this.RestClient.PostAsync<TimeEntry, InvokeResult>("/api/time/entry", timeEntry);
                    if (result.Successful)
                    {
                        TimeEntryTask = null;
                    }
                    else
                    {
                        await Popups.ShowAsync(result.Errors.First().Message);
                    }
                }
                else
                {
                    await Popups.ShowAsync("Please enter a validate number of hours.");
                }
            });
        }

        async void SaveNewTask()
        {
            var errors = new StringBuilder();

            if (NewWorkTask == null)
            {
                return;
            }

            if (EntityHeader.IsNullOrEmpty(NewWorkTask.Project))
            {
                errors.AppendLine("Project is required.");
            }

            if (EntityHeader.IsNullOrEmpty(NewWorkTask.TaskType))
            {
                errors.AppendLine("Task Type is required.");
            }

            if (String.IsNullOrEmpty(NewWorkTask.Name))
            {
                errors.AppendLine("Task Name is required.");
            }

            if (String.IsNullOrEmpty(NewWorkTask.Description))
            {
                errors.AppendLine("Description is required.");
            }

            if (errors.Length > 0)
            {
                await Popups.ShowAsync(errors.ToString());
                return;
            }

            await PerformNetworkOperation(async () =>
            {
                var result = await this.RestClient.PostAsync<NewTask, WorkTaskSummary>($"/api/tasks/add", NewWorkTask);
                if (result.Successful)
                {
                    var summary = result.Result;
                    AllTasks.Add(summary);
                    NewWorkTask = null;
                    SelectedProjectForNewTask = null;
                    this.FilterTasks();
                }
                return result.ToInvokeResult();
            });

        }

        async void RefreshTasks()
        {
            await PerformNetworkOperation(async () =>
            {
                await RefreshTasksInBackground(true);
                FilterTasks();
            });
        }

        void OpenFullTask(WorkTaskSummary summary)
        {
            var taskPath = $"/ngx/#pm/{summary.ProjectId}/task/{summary.Id}";

            switch (AppConfig.Environment)
            {
                case Environments.Production:
                case Environments.Beta:
                    Services.Network.OpenURI(new System.Uri($"https://www.nuviot.com{taskPath}"));
                    break;
                case Environments.Local:
                case Environments.LocalDevelopment:
                    Services.Network.OpenURI(new System.Uri($"http://localhost:5000{taskPath}"));
                    break;

                case Environments.Development:
                    Services.Network.OpenURI(new System.Uri($"https://dev.nuviot.com{taskPath}"));
                    break;

                case Environments.Staging:
                case Environments.Testing:
                    Services.Network.OpenURI(new System.Uri($"https://test.nuviot.com{taskPath}"));
                    break;
            }
        }

        private EntityHeader EntityHeaderAll()
        {
            return EntityHeader.Create("all", "All");
        }

        #region Filter/Options Lists
        List<StatusConfiguration> _statusConfigurations;

        List<EntityHeader> _assignedTaskLead;
        public List<EntityHeader> AssignedTaskLeads
        {
            get => _assignedTaskLead;

            set
            {
                value.Insert(0, EntityHeaderAll());
                Set(ref _assignedTaskLead, value);
            }
        }

        List<EntityHeader> _assignedPrimaryContributorLead;
        public List<EntityHeader> AssignedPrimaryContributor
        {
            get => _assignedPrimaryContributorLead;
            set
            {
                value.Insert(0, EntityHeaderAll());
                Set(ref _assignedPrimaryContributorLead, value);
            }
        }

        List<EntityHeader> _assignedQaLead;
        public List<EntityHeader> AssignedQALeads
        {
            get => _assignedQaLead;
            set
            {
                value.Insert(0, EntityHeaderAll());
                Set(ref _assignedQaLead, value);
            }
        }

        List<EntityHeader> _statusFilter;
        public List<EntityHeader> StatusFilter
        {
            get => _statusFilter;
            set
            {
                value.Insert(0, EntityHeaderAll());
                Set(ref _statusFilter, value);
            }
        }

        List<EntityHeader> _filteredViews;
        public List<EntityHeader> FilteredViews
        {
            get => _filteredViews;
            set
            {
                value.Insert(0, EntityHeaderAll());
                Set(ref _filteredViews, value);
            }
        }

        List<WorkTaskType> _workTaskTypes;

        List<EntityHeader> _proejcts;
        public List<EntityHeader> Projects
        {
            get => _proejcts;
            set
            {
                value.Insert(0, EntityHeaderAll());
                Set(ref _proejcts, value);
            }
        }


        List<EntityHeader> _modules;
        public List<EntityHeader> Modules
        {
            get => _modules;
            set
            {
                value.Insert(0, EntityHeaderAll());
                Set(ref _modules, value);
            }
        }

        List<EntityHeader> _workTaskTypesFilter;
        public List<EntityHeader> WorkTaskTypesFilter
        {
            get => _workTaskTypesFilter;
            set
            {
                value.Insert(0, EntityHeaderAll());
                Set(ref _workTaskTypesFilter, value);
            }
        }

        private List<ProjectSummary> _projects;
        public List<ProjectSummary> AllProjects
        {
            get => _projects;
            set { Set(ref _projects, value); }
        }

        private List<EntityHeader> _projectsForNewTask;
        public List<EntityHeader> ProjectsForNewTask
        {
            get => _projectsForNewTask;
            set { Set(ref _projectsForNewTask, value); }
        }


        private List<EntityHeader> _modulesForNewTask;
        public List<EntityHeader> ModulesForNewTask
        {
            get => _modulesForNewTask;
            set { Set(ref _modulesForNewTask, value); }
        }

        private List<EntityHeader> _workTaskTypesForNewTask;
        public List<EntityHeader> WorkTaskTypesForNewTask
        {
            get => _workTaskTypesForNewTask;
            set { Set(ref _workTaskTypesForNewTask, value); }
        }

        private EntityHeader _selectedProjectForNewTask;
        public EntityHeader SelectedProjectForNewTask
        {
            get => _selectedProjectForNewTask;
            set
            {
                Set(ref _selectedProjectForNewTask, value);
                if (NewWorkTask != null)
                {
                    NewWorkTask.Project = value;
                }

                if (value != null)
                {
                    var project = AllProjects.FirstOrDefault(prj => prj.Id == value.Id);
                    if (project != null)
                    {
                        ModulesForNewTask = project.Modules;
                    }
                }
            }
        }

        private List<UserInfoSummary> _allUsers;
        public List<UserInfoSummary> AllUsers
        {
            get => _allUsers;
            set { Set(ref _allUsers, value); }
        }


        ObservableCollection<WorkTaskSummary> _tasks;
        public ObservableCollection<WorkTaskSummary> AllTasks
        {
            get => _tasks;
            set
            {
                if (_tasks != null && value != null)
                {
                    if (_tasks.GetHashCode() == value.GetHashCode())
                    {
                        return;
                    }
                }

                Set(ref _tasks, value);
            }
        }

        ObservableCollection<WorkTaskSummary> _filteredTasks;
        public ObservableCollection<WorkTaskSummary> FilteredTasks
        {
            get => _filteredTasks;
            set { Set(ref _filteredTasks, value); }
        }
        #endregion

        #region View Load Methods
        public async Task<InvokeResult> TryLoadListsFromCacheAsync()
        {
            var scs = await this.RestClient.TryGetFromCache<StatusConfiguration>("/api/pm/statusconfigurations");
            if (!scs.Successful)
                return scs;

            _statusConfigurations = scs.Model.ToList();

            var wtts = await this.RestClient.TryGetFromCache<WorkTaskType>("/api/pm/worktasktypes");
            if (!wtts.Successful)
                return wtts;

            _workTaskTypes = wtts.Model.ToList();
            WorkTaskTypesFilter = _workTaskTypes.Select(wtt => EntityHeader.Create(wtt.Id, wtt.Key, wtt.Name)).Distinct().ToList();
            WorkTaskTypesForNewTask = _workTaskTypes.Select(wtt => EntityHeader.Create(wtt.Id, wtt.Key, wtt.Name)).ToList();

            var views = await this.RestClient.TryGetFromCache<KanbanView>("/api/pm/kanbanviews");
            if (!views.Successful)
                return views;

            FilteredViews = views.Model.Select(kb => EntityHeader.Create(kb.Id, kb.Key, kb.Name)).ToList();

            var aps = await this.RestClient.TryGetFromCache<ProjectSummary>("/api/projects");
            if (!aps.Successful)
                return aps;

            AllProjects = aps.Model.ToList();
            ProjectsForNewTask = AllProjects.Select(prj => EntityHeader.Create(prj.Id, prj.Key, prj.Name)).ToList();


            var users = await this.RestClient.TryGetFromCache<UserInfoSummary>("/api/users/active");
            if (!users.Successful)
                return users;

            AllUsers = users.Model.ToList();

            var lastFilter = await Storage.GetKVPAsync<string>("LAST_FILTER_ID", FilteredViews[0].Id);
            SelectedKanbanView = FilteredViews.Single(val => val.Id == lastFilter);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> LoadCommonSettingsAsync()
        {
            var views = await this.RestClient.GetListResponseAsync<KanbanView>("/api/pm/kanbanviews");
            Status = "loading all view selectors";

            _statusConfigurations = (await this.RestClient.GetListResponseAsync<StatusConfiguration>("/api/pm/statusconfigurations")).Model.ToList();
            Status = "loading status configurations";

            _workTaskTypes = (await this.RestClient.GetListResponseAsync<WorkTaskType>("/api/pm/worktasktypes")).Model.ToList();
            WorkTaskTypesForNewTask = _workTaskTypes.Select(wtt => EntityHeader.Create(wtt.Id, wtt.Name)).ToList();
            Status = "loading work task types";

            WorkTaskTypesFilter = _workTaskTypes.Select(wtt => EntityHeader.Create(wtt.Id, wtt.Name)).Distinct().ToList();
            FilteredViews = views.Model.Select(kb => EntityHeader.Create(kb.Id, kb.Name)).ToList();
            AllProjects = (await this.RestClient.GetListResponseAsync<ProjectSummary>("/api/projects")).Model.ToList();
            ProjectsForNewTask = AllProjects.Select(prj => EntityHeader.Create(prj.Id, prj.Name)).ToList();

            Status = "loading all projects";
            AllUsers = (await this.RestClient.GetListResponseAsync<UserInfoSummary>("/api/users/active")).Model.ToList();
            Status = "loading all users";

            var lastFilter = await Storage.GetKVPAsync<string>("LAST_FILTER_ID", FilteredViews[0].Id);
            SelectedKanbanView = FilteredViews.Single(val => val.Id == lastFilter);

            Status = string.Empty;

            return views.ToInvokeResult();
        }

        public async void LoadCommonSettingsInBackground()
        {
            await TryLoadListsFromCacheAsync();
        }

        public async Task RefreshTasksFromServerAsync()
        {
            await PerformNetworkOperation(async () =>
            {
                await RefreshTasksInBackground();
            });
        }

        private void PopualteFilterSelections(bool maintainFilters = false)
        {
            var prjFilter = SelectedProjectFilter;
            var modFilter = SelectedModuleFilter;
            var statusFilter = SelectedStatusFilter;
            var wtf = SelectedWorKTypeFilter;
            var lead = SelectedTaskLeadFilter;
            var ctrb = SelectedPrimaryContributorFilter;
            var qa = SelectedQAResourceFilter;

            Projects = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.ProjectId)).Select(prj => EntityHeader.Create(prj.ProjectId, prj.Key, prj.ProjectName)).ToList().Distinct().ToList();
            Modules = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.ModuleId)).Select(tsk => EntityHeader.Create(tsk.ModuleId, tsk.Key, tsk.ModuleName)).ToList().Distinct().ToList();
            AssignedPrimaryContributor = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.PrimaryContributorId)).Select(tsk => EntityHeader.Create(tsk.PrimaryContributorId, tsk.PrimaryContributor)).ToList().Distinct().ToList();
            AssignedQALeads = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.QaResourceId)).Select(tsk => EntityHeader.Create(tsk.QaResourceId, tsk.QaResource)).ToList().Distinct().ToList();
            AssignedTaskLeads = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.AssignedToUserId)).Select(tsk => EntityHeader.Create(tsk.AssignedToUserId, tsk.AssignedToUser)).ToList().Distinct().ToList();
            StatusFilter = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.PrimaryContributorId)).Select(tsk => EntityHeader.Create(tsk.StatusId, tsk.Status)).ToList().Distinct().ToList();

            if (maintainFilters)
            {
                _selectedProjectFilter = prjFilter;
                _selectedModuleFilter = modFilter;
                _selectedStatusFilter = statusFilter;
                _selectedWorkTypeFilter = wtf;

                _selectedTaskLeadFilter = lead;
                _selectedPrimaryContributorFilter = ctrb;
                _selectedQAResourceFilter = qa;
            }
            else
            {
                _selectedProjectFilter = Projects[0];
                _selectedModuleFilter = Modules[0];
                _selectedStatusFilter = StatusFilter[0];
                _selectedWorkTypeFilter = WorkTaskTypesFilter[0];

                _selectedQAResourceFilter = AssignedQALeads[0];
                _selectedTaskLeadFilter = AssignedTaskLeads[0];
                _selectedPrimaryContributorFilter = AssignedPrimaryContributor[0];
            }

            RaisePropertyChanged(nameof(SelectedProjectFilter));
            RaisePropertyChanged(nameof(SelectedModuleFilter));
            RaisePropertyChanged(nameof(SelectedStatusFilter));
            RaisePropertyChanged(nameof(SelectedWorKTypeFilter));

            RaisePropertyChanged(nameof(SelectedTaskLeadFilter));
            RaisePropertyChanged(nameof(SelectedQAResourceFilter));
            RaisePropertyChanged(nameof(SelectedPrimaryContributorFilter));
        }

        public async Task RefreshTasksInBackground(bool maintainFilters = false)
        {
            if (SelectedKanbanView != null && SelectedKanbanView.Id != "all")
            {
                try
                {
                    var taskResposne = await this.RestClient.GetListResponseAsync<WorkTaskSummary>($"/api/pm/tasks/view/{SelectedKanbanView.Id}");

                    if (taskResposne.Successful)
                    {
                        Status = "loading work tasks in background";
                        await Storage.StoreKVP("LAST_FILTER_ID", SelectedKanbanView.Id);


                        AllTasks = new ObservableCollection<WorkTaskSummary>(taskResposne.Model);
                        PopualteFilterSelections(maintainFilters);

                        Status = string.Empty;
                    }
                }

                catch (CouldNotRenewTokenException)
                {
                    IsBusy = false;
                    await ForceLogoutAsync();
                }
            }
            else
            {
                AllTasks = new ObservableCollection<WorkTaskSummary>();
            }
        }

        public async void TryRefreshTasksFromCache()
        {
            var taskResposne = await this.RestClient.TryGetFromCache<WorkTaskSummary>($"/api/pm/tasks/view/{SelectedKanbanView.Id}");

            if (taskResposne.Successful)
            {
                await Storage.StoreKVP("LAST_FILTER_ID", SelectedKanbanView.Id);
                AllTasks = new ObservableCollection<WorkTaskSummary>(taskResposne.Model);
                PopualteFilterSelections(false);
                await RefreshTasksInBackground();
            }
            else
            {
                await RefreshTasksFromServerAsync();
            }
        }
        #endregion

        public void FilterTasks()
        {
            Console.WriteLine("performing filter on task.");
            if (AllTasks != null)
            {
                var filteredTasks = new ObservableCollection<WorkTaskSummary>(AllTasks.Where(
                    tsk =>
                    (SelectedProjectFilter != null && (tsk.ProjectId == SelectedProjectFilter.Id || SelectedProjectFilter.Id == "all")) &&
                    (SelectedStatusFilter != null && (tsk.StatusId == SelectedStatusFilter.Id || SelectedStatusFilter.Id == "all")) &&
                    (SelectedWorKTypeFilter != null && (tsk.TaskTypeId == SelectedWorKTypeFilter.Id || SelectedWorKTypeFilter.Id == "all")) &&
                    (SelectedModuleFilter != null && (tsk.ModuleId == SelectedModuleFilter.Id || SelectedModuleFilter.Id == "all")) &&
                    (ShowOnlyMyWork == false || tsk.AssignedToUserId == AuthManager.User.Id || tsk.QaResourceId == AuthManager.User.Id || tsk.PrimaryContributorId == AuthManager.User.Id)
                    ));

                if (FilteredTasks == null || FilteredTasks.GetHashCode() != filteredTasks.GetHashCode())
                    FilteredTasks = filteredTasks;
            }
        }

        #region Selected Filter Values
        EntityHeader _selectedWorkTypeFilter;
        public EntityHeader SelectedWorKTypeFilter
        {
            get => _selectedWorkTypeFilter;
            set
            {
                Set(ref _selectedWorkTypeFilter, value);
                FilterTasks();
            }
        }

        EntityHeader _selectedQAResourceFilter;
        public EntityHeader SelectedQAResourceFilter
        {
            get => _selectedQAResourceFilter;
            set
            {
                Set(ref _selectedQAResourceFilter, value);
                FilterTasks();
            }
        }

        EntityHeader _selectedModuleFilter;
        public EntityHeader SelectedModuleFilter
        {
            get => _selectedModuleFilter;
            set
            {
                Set(ref _selectedModuleFilter, value);
                FilterTasks();
            }
        }

        EntityHeader _selectedProjectFilter;
        public EntityHeader SelectedProjectFilter
        {
            get => _selectedProjectFilter;
            set
            {
                Set(ref _selectedProjectFilter, value);
                FilterTasks();
            }
        }

        EntityHeader _selectedTaskLeadFilter;
        public EntityHeader SelectedTaskLeadFilter
        {
            get => _selectedTaskLeadFilter;
            set
            {
                Set(ref _selectedTaskLeadFilter, value);
                FilterTasks();
            }
        }

        EntityHeader _selectedPrimaryContributorFilter;
        public EntityHeader SelectedPrimaryContributorFilter
        {
            get => _selectedPrimaryContributorFilter;
            set
            {
                Set(ref _selectedPrimaryContributorFilter, value);
                FilterTasks();
            }
        }

        EntityHeader _selectedStatusFilter;
        public EntityHeader SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                Set(ref _selectedStatusFilter, value);
                FilterTasks();
            }
        }

        EntityHeader _kanbanView;
        public EntityHeader SelectedKanbanView
        {
            get => _kanbanView;
            set
            {
                var prevValue = _kanbanView == null ? string.Empty : _kanbanView.Id;
                if (Set(ref _kanbanView, value))
                {
                    if (_kanbanView != null && _kanbanView.Id != prevValue)
                        TryRefreshTasksFromCache();
                }
            }
        }

        private bool _showOnlyMyWork;
        public bool ShowOnlyMyWork
        {
            get => _showOnlyMyWork;
            set
            {
                Set(ref _showOnlyMyWork, value);
                FilterTasks();
            }
        }

        public string TimeEntryDateDisplay
        {
            get => _timeEntryDate.ToLongDateString();
        }

        WorkTaskSummary _timeEntryTask;
        public WorkTaskSummary TimeEntryTask
        {
            get => _timeEntryTask;
            set => Set(ref _timeEntryTask, value);
        }

        private DateTime _timeEntryDate;
        public DateTime TimeEntryDate
        {
            get => _timeEntryDate;
            set
            {
                Set(ref _timeEntryDate, value);
                RaisePropertyChanged(nameof(TimeEntryDateDisplay));
            }
        }

        private string _timeEntryHours;
        public string TimeEntryHours
        {
            get => _timeEntryHours;
            set => Set(ref _timeEntryHours, value);
        }

        private string _timeEntryNotes;
        public string TimeEntryNotes
        {
            get => _timeEntryNotes;
            set => Set(ref _timeEntryNotes, value);
        }
        #endregion

        WorkTaskSummary _taskSummary;
        public WorkTaskSummary TaskSummary
        {
            get { return _taskSummary; }
            set
            {
                Set(ref _taskSummary, value);
                if (_taskSummary != null)
                {
                    var config = _statusConfigurations.SingleOrDefault(sc => sc.Id == _taskSummary.StatusConfigurationId);
                    if (config != null)
                    {
                        var currentOption = config.Options.SingleOrDefault(opt => opt.Key == _taskSummary.StatusId);
                        if (currentOption != null)
                        {
                            var transitions = currentOption.ValidTransitions.ToList();
                            var defaultOption = new StatusTransition { Name = "-transition to new status-", Id = "-1", Key = "-1" };
                            transitions.Insert(0, defaultOption);
                            AvailableStatusTransitions = transitions;
                            SelectedTransition = defaultOption;
                        }
                    }

                    if (!String.IsNullOrEmpty(_taskSummary.ExternalStatusConfigurationId))
                    {
                        var externalConfig = _statusConfigurations.SingleOrDefault(sc => sc.Id == _taskSummary.ExternalStatusConfigurationId);
                        if (externalConfig != null)
                        {
                            var currentOption = externalConfig.Options.SingleOrDefault(opt => opt.Key == _taskSummary.ExternalStatusId);
                            if (currentOption != null)
                            {
                                var transitions = currentOption.ValidTransitions.ToList();
                                var defaultOption = new StatusTransition { Name = "-transition to new external status-", Id = "-1", Key = "-1" };
                                transitions.Insert(0, defaultOption);
                                AvailableExternalStatusTransitions = transitions;
                                SelectedExternalTransition = defaultOption;
                            }
                        }
                    }
                    else
                    {
                        AvailableExternalStatusTransitions = null;
                    }
                }
            }
        }


        StatusTransition _selectedExternalTransition;
        public StatusTransition SelectedExternalTransition
        {
            get => _selectedExternalTransition;
            set { Set(ref _selectedExternalTransition, value); }
        }

        private string _statusUpdateNotes;
        public string StatusUpdateNotes
        {
            get => _statusUpdateNotes;
            set => Set(ref _statusUpdateNotes, value);
        }

        StatusTransition _selectedTransition;
        public StatusTransition SelectedTransition
        {
            get => _selectedTransition;
            set { Set(ref _selectedTransition, value); }
        }

        List<StatusTransition> _availableStatusTransitions;
        public List<StatusTransition> AvailableStatusTransitions
        {
            get => _availableStatusTransitions;
            set
            {
                Set(ref _availableStatusTransitions, value);
            }
        }

        List<StatusTransition> _availableExternalStatusTransitions;
        public List<StatusTransition> AvailableExternalStatusTransitions
        {
            get => _availableExternalStatusTransitions;
            set
            {
                Set(ref _availableExternalStatusTransitions, value);
            }
        }


        string _status = "ready";
        public string Status
        {
            get => _status;
            set { Set(ref _status, value); }
        }

        NewTask _newTask;
        public NewTask NewWorkTask
        {
            get => _newTask;
            set => Set(ref _newTask, value);
        }

        #region Commands
        public RelayCommand AddTaskCommand { get; private set; }
        public RelayCommand<WorkTaskSummary> OpenFullTaskCommand { get; private set; }
        public RelayCommand CloseStatusUpdateCommand { get; private set; }
        public RelayCommand SaveNewTaskCommand { get; private set; }
        public RelayCommand CancelNewTaskCommand { get; private set; }
        public RelayCommand RefreshTasksCommand { get; private set; }

        public RelayCommand SaveNewTimeCommand { get; private set; }
        public RelayCommand CancelNewTimeCommand { get; private set; }

        public RelayCommand TimeEntryPrevDateCommand { get; private set; }

        public RelayCommand TimeEntryNextDateCommand { get; private set; }

        public RelayCommand<WorkTaskSummary> AddRelatedTaskCommand { get; private set; }
        public RelayCommand<WorkTaskSummary> AddTimeCommand { get; private set; }

        public RelayCommand AddExpectedOutcomeCommand { get; }
        #endregion

        public override async Task InitAsync()
        {
            if ((await TryLoadListsFromCacheAsync()).Successful)
            {
                LoadCommonSettingsInBackground();
            }
            else
                await PerformNetworkOperation(LoadCommonSettingsAsync);
        }
    }
}