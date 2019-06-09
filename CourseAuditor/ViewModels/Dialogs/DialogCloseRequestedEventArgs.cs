﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseAuditor.ViewModels.Dialogs
{
    public class DialogCloseRequestedEventArgs : EventArgs
    {
        public DialogCloseRequestedEventArgs(bool? dialogResult)
        {
            DialogResult = dialogResult;
        }

        public bool? DialogResult { get; }
    }
}
