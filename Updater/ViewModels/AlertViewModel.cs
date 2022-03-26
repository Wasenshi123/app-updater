using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace Updater.ViewModels
{
    public class AlertViewModel : ViewModelBase
    {
        private string message;
        public string Message { get => message; set => this.RaiseAndSetIfChanged(ref message, value); }

        public ReactiveCommand<Unit, Unit> Ok { get; }

        public AlertViewModel(string msg)
        {
            Message = msg;
            Ok = ReactiveCommand.Create(() => { });
        }
    }
}
