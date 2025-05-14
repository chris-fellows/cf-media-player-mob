using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.ViewModels
{
    /// <summary>
    /// Base page model
    /// </summary>
    public abstract class PageModelBase
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public LocalizationResources LocalizationResources => LocalizationResources.Instance;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
                     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        /// <summary>
        /// Event for general error
        /// </summary>
        /// <param name="exception"></param>
        public delegate void GeneralError(Exception exception);
        public event GeneralError? OnGeneralError;

        /// <summary>
        /// Event for debug
        /// </summary>
        /// <param name="debug"></param>
        public delegate void DebugAction(string debug);
        public event DebugAction? OnDebugAction;       
        
        public void RaiseOnGeneralError(Exception exception)
        {
            if (OnGeneralError != null)
            {
                OnGeneralError(exception);
            }
        }

        public void RaiseOnDebugAction(string debug)
        {
            if (OnDebugAction != null)
            {
                OnDebugAction(debug);
            }
        }
    }
}
