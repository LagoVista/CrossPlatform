using BugLog.Models;
using LagoVista.Client.Core.Resources;
using LagoVista.Client.Core.ViewModels;
using LagoVista.Client.Core.ViewModels.Other;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.ProjectManagement.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        }

        void CloseStatusWindow()
        {
            TaskSummary = null;
        }

        void AddTask()
        {

        }


        #region Filter/Options Lists
        List<StatusConfiguration> _statusConfigurations;

        List<PickerItem> _assignedTaskLead;
        public List<PickerItem> AssignedTaskLeads
        {
            get => _assignedTaskLead;

            set
            {
                value.Insert(0, PickerItem.All());
                Set(ref _assignedTaskLead, value);
                SelectedTaskLeadFilter = value[0];
            }
        }

        List<PickerItem> _assignedPrimaryContributorLead;
        public List<PickerItem> AssignedPrimaryContributor
        {
            get => _assignedPrimaryContributorLead;
            set
            {
                value.Insert(0, PickerItem.All());
                Set(ref _assignedPrimaryContributorLead, value);
                SelectedPrimaryContributorFilter = value[0];
            }
        }

        List<PickerItem> _assignedQaLead;
        public List<PickerItem> AssignedQALeads
        {
            get => _assignedQaLead;
            set
            {
                value.Insert(0, PickerItem.All());
                Set(ref _assignedQaLead, value);
                SelectedQAResourceFilter = value[0];
            }
        }

        List<PickerItem> _statusFilter;
        public List<PickerItem> StatusFilter
        {
            get => _statusFilter;
            set
            {
                value.Insert(0, PickerItem.All());
                Set(ref _statusFilter, value);
                SelectedStatusFilter = value[0];
            }
        }

        List<PickerItem> _filteredViews;
        public List<PickerItem> FilteredViews
        {
            get => _filteredViews;
            set
            {
                value.Insert(0, PickerItem.All());
                Set(ref _filteredViews, value);
            }
        }



        List<WorkTaskType> _workTaskTypes;

        List<PickerItem> _proejcts;
        public List<PickerItem> Projects
        {
            get => _proejcts;
            set
            {
                value.Insert(0, PickerItem.All());
                Set(ref _proejcts, value);
                SelectedProjectFilter = value[0];
            }
        }

        List<PickerItem> _modules;
        public List<PickerItem> Modules
        {
            get => _modules;
            set
            {
                value.Insert(0, PickerItem.All());
                Set(ref _modules, value);
                SelectedModuleFilter = value[0];
            }
        }

        List<PickerItem> _workTaskTypesFilter;
        public List<PickerItem> WorkTaskTypesFilter
        {
            get => _workTaskTypesFilter;
            set
            {
                value.Insert(0, PickerItem.All());
                Set(ref _workTaskTypesFilter, value);
                SelectedWorKTypeFilter = value[0];
            }
        }

        private List<ProjectSummary> _projects;
        public List<ProjectSummary> AllProjects
        {
            get => _projects;
            set { Set(ref _projects, value); }
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

        public async Task<InvokeResult> TryLoadListsFromCacheAsync()
        {
            var views = await this.RestClient.TryGetFromCache<KanbanView>("/api/pm/kanbanviews");
            _statusConfigurations = (await this.RestClient.TryGetFromCache<StatusConfiguration>("/api/pm/statusconfigurations")).Model.ToList();

            _workTaskTypes = (await this.RestClient.TryGetFromCache<WorkTaskType>("/api/pm/worktasktypes")).Model.ToList();
            WorkTaskTypesFilter = _workTaskTypes.Select(wtt => PickerItem.Create(wtt.Key, wtt.Name)).Distinct().ToList();
            FilteredViews = views.Model.Select(kb => PickerItem.Create(kb.Id, kb.Name)).ToList();
            AllProjects = (await this.RestClient.TryGetFromCache<ProjectSummary>("/api/projects")).Model.ToList();
            AllUsers = (await this.RestClient.TryGetFromCache<UserInfoSummary>("/api/users/active")).Model.ToList();

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
            Status = "loading work task types";

            WorkTaskTypesFilter = _workTaskTypes.Select(wtt => PickerItem.Create(wtt.Key, wtt.Name)).Distinct().ToList();
            FilteredViews = views.Model.Select(kb => PickerItem.Create(kb.Id, kb.Name)).ToList();
            AllProjects = (await this.RestClient.GetListResponseAsync<ProjectSummary>("/api/projects")).Model.ToList();

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

        public async Task RefreshTasksInBackground()
        {
            if (SelectedKanbanView != null && SelectedKanbanView.Id != "all")
            {
                var taskResposne = await this.RestClient.GetListResponseAsync<WorkTaskSummary>($"/api/pm/tasks/view/{SelectedKanbanView.Id}");

                if (taskResposne.Successful)
                {
                    Status = "loading work tasks in background";
                    await Storage.StoreKVP("LAST_FILTER_ID", SelectedKanbanView.Id);
                    AllTasks = new ObservableCollection<WorkTaskSummary>(taskResposne.Model);
                    Projects = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.ProjectId)).Select(tsk => PickerItem.Create(tsk.ProjectId, tsk.ProjectName)).ToList().Distinct().ToList();
                    Modules = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.ModuleId)).Select(tsk => PickerItem.Create(tsk.ModuleId, tsk.ModuleName)).ToList().Distinct().ToList();
                    AssignedPrimaryContributor = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.PrimaryContributorId)).Select(tsk => PickerItem.Create(tsk.PrimaryContributorId, tsk.PrimaryContributor)).ToList().Distinct().ToList();
                    AssignedQALeads = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.QaResourceId)).Select(tsk => PickerItem.Create(tsk.QaResourceId, tsk.QaResource)).ToList().Distinct().ToList();
                    AssignedTaskLeads = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.AssignedToUserId)).Select(tsk => PickerItem.Create(tsk.AssignedToUserId, tsk.AssignedToUser)).ToList().Distinct().ToList();
                    StatusFilter = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.PrimaryContributorId)).Select(tsk => PickerItem.Create(tsk.StatusId, tsk.Status)).ToList().Distinct().ToList();
                    FilteredTasks = AllTasks;
                    Status = string.Empty;
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
                Projects = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.ProjectId)).Select(tsk => PickerItem.Create(tsk.ProjectId, tsk.ProjectName)).ToList().Distinct().ToList();
                Modules = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.ModuleId)).Select(tsk => PickerItem.Create(tsk.ModuleId, tsk.ModuleName)).ToList().Distinct().ToList();
                AssignedPrimaryContributor = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.PrimaryContributorId)).Select(tsk => PickerItem.Create(tsk.PrimaryContributorId, tsk.PrimaryContributor)).ToList().Distinct().ToList();
                AssignedQALeads = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.QaResourceId)).Select(tsk => PickerItem.Create(tsk.QaResourceId, tsk.QaResource)).ToList().Distinct().ToList();
                AssignedTaskLeads = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.AssignedToUserId)).Select(tsk => PickerItem.Create(tsk.AssignedToUserId, tsk.AssignedToUser)).ToList().Distinct().ToList();
                StatusFilter = AllTasks.Where(tsk => !string.IsNullOrEmpty(tsk.PrimaryContributorId)).Select(tsk => PickerItem.Create(tsk.StatusId, tsk.Status)).ToList().Distinct().ToList();
                FilteredTasks = AllTasks;
                await RefreshTasksInBackground();
            }
            else
            {
                await RefreshTasksFromServerAsync();
            }
        }

        public void FilterTasks()
        {
            if (AllTasks != null)
            {
                FilteredTasks = new ObservableCollection<WorkTaskSummary>(AllTasks.Where(
                    tsk =>
                    (SelectedProjectFilter != null && (tsk.ProjectId == SelectedProjectFilter.Id || SelectedProjectFilter.IsDefault)) &&
                    (SelectedStatusFilter != null && (tsk.StatusId == SelectedStatusFilter.Id || SelectedStatusFilter.IsDefault)) &&
                    (SelectedWorKTypeFilter != null && (tsk.TaskTypeId == SelectedWorKTypeFilter.Id || SelectedWorKTypeFilter.IsDefault)) &&
                    (SelectedModuleFilter != null && (tsk.ModuleId == SelectedModuleFilter.Id || SelectedModuleFilter.IsDefault))
                    ));
            }
        }

        #region Selected Filter Values
        PickerItem _selectedWorkTypeFilter;
        public PickerItem SelectedWorKTypeFilter
        {
            get => _selectedWorkTypeFilter;
            set
            {
                Set(ref _selectedWorkTypeFilter, value);
                FilterTasks();
            }
        }


        PickerItem _selectedQAResourceFilter;
        public PickerItem SelectedQAResourceFilter
        {
            get => _selectedQAResourceFilter;
            set
            {
                Set(ref _selectedQAResourceFilter, value);
                FilterTasks();
            }
        }


        PickerItem _selectedModuleFilter;
        public PickerItem SelectedModuleFilter
        {
            get => _selectedModuleFilter;
            set
            {
                Set(ref _selectedModuleFilter, value);
                FilterTasks();
            }
        }

        PickerItem _selectedProjectFilter;
        public PickerItem SelectedProjectFilter
        {
            get => _selectedProjectFilter;
            set
            {
                Set(ref _selectedProjectFilter, value);
                FilterTasks();
            }
        }

        PickerItem _selectedTaskLeadFilter;
        public PickerItem SelectedTaskLeadFilter
        {
            get => _selectedTaskLeadFilter;
            set
            {
                Set(ref _selectedTaskLeadFilter, value);
                FilterTasks();
            }
        }


        PickerItem _selectedPrimaryContributorFilter;
        public PickerItem SelectedPrimaryContributorFilter
        {
            get => _selectedPrimaryContributorFilter;
            set
            {
                Set(ref _selectedPrimaryContributorFilter, value);
                FilterTasks();
            }
        }

        PickerItem _selectedStatusFilter;
        public PickerItem SelectedStatusFilter
        {
            get => _selectedStatusFilter;
            set
            {
                Set(ref _selectedStatusFilter, value);
                FilterTasks();
            }
        }

        PickerItem _kanbanView;
        public PickerItem SelectedKanbanView
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

        WorkTaskSummary _taskSummary;
        public WorkTaskSummary TaskSummary
        {
            get { return _taskSummary; }
            set
            {
                Set(ref _taskSummary, value);

            }
        }

        string _status = "ready";
        public string Status
        {
            get => _status;
            set { Set(ref _status, value); }
        }

        #endregion

        #region Commands
        public RelayCommand AddTaskCommand { get; private set; }

        public RelayCommand CloseStatusUpdateCommand { get; private set; }
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