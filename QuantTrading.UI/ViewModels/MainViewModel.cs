using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuantTrading.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _title = "CIB Trading Portfolio - En attente de flux...";

        public MainViewModel()
        {
            // On injectera les services ici plus tard
        }
    }
}
