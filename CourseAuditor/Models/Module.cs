﻿using CourseAuditor.DAL;
using CourseAuditor.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Data.Entity;

namespace CourseAuditor.Models
{
    public class Module : ObservableObject
    {
        private Group _Group;
        private int _Number;
        private DateTime _DateStart;
        private DateTime _DateEnd;

        [NotMapped]
        public ICollection<Student> Students
        {
            get
            {
                using (var _context = new ApplicationContext())
                {
                    return _context.Students
                        .Where(x => (x.Group.ID == _Group.ID) && (x.DateStart >= _DateStart && x.DateStart < _DateEnd))
                        .Include(t => t.Person)
                        .ToList();
                }
            }
        }


        public virtual Group Group
        {
            get
            {
                return _Group;
            }
            set
            {
                _Group = value;
                OnPropertyChanged("Group");
            }
        }

        public int Number
        {
            get
            {
                return _Number;
            }
            set
            {
                _Number = value;
                OnPropertyChanged("Number");
            }
        }
        public DateTime DateStart
        {
            get
            {
                return _DateStart;
            }
            set
            {
                _DateStart = value;
                OnPropertyChanged("DateStart");
            }
        }
        public DateTime DateEnd
        {
            get
            {
                return _DateEnd;
            }
            set
            {
                _DateEnd = value;
                OnPropertyChanged("DateEnd");
            }
        }

        public override string ToString()
        {
            return "Модуль " + Number.ToString();
        }
    }
}