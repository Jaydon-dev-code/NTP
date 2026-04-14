using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using HandyControl.Data;
using SL.MLineDataPrecisionTracking.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SL.MLineDataPrecisionTracking.Client.ViewModel.Control
{
    public partial class MeticulousPursuitViewModel : ObservableObject
    {
        private PaginationPage _paginationPage;
        public PaginationPage PaginationPage
        {
            get => _paginationPage;
            set => SetProperty(ref _paginationPage, value);
        }

        private RefinedSearchCriteria _queryConditions;
        public RefinedSearchCriteria QueryConditions
        {
            get => _queryConditions;
            set => SetProperty(ref _queryConditions, value);
        }



        private RelayCommand _queryCommand;
        public IRelayCommand QueryCommand
        {
            get
            {
                if (_queryCommand == null)
                {
                    _queryCommand = new RelayCommand(Query);
                }
                return _queryCommand;
            }
        }

        private RelayCommand _saveQueryCommand;
        public IRelayCommand SaveQueryCommand
        {
            get
            {
                if (_saveQueryCommand == null)
                {
                    _saveQueryCommand = new RelayCommand(SaveQuery);
                }
                return _saveQueryCommand;
            }
        }

        private RelayCommand _pageUpdatedCommand;
        public IRelayCommand PageUpdatedCommand
        {
            get
            {
                if (_pageUpdatedCommand == null)
                {
                    _pageUpdatedCommand = new RelayCommand(PageUpdated);
                }
                return _pageUpdatedCommand;
            }
        }
        public MeticulousPursuitViewModel() {
            PaginationPage = new PaginationPage();
       var lastDay=     DateTime.Now.Date.AddDays(-1);
            QueryConditions = new RefinedSearchCriteria() { 
                StartDate= lastDay.Date,
                EndDate= lastDay.Date,
                StartTime= lastDay,
                EndTime= lastDay.AddTicks(-1),
                Op ="ALL", 
                Result="ALL" };
        }


         void Query()
        {
          
        }

  
         void SaveQuery()
        { }
      
        void PageUpdated()
        { }
    }
}
