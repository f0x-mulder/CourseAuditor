﻿using CourseAuditor.DAL;
using CourseAuditor.Helpers;
using CourseAuditor.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CourseAuditor.ViewModels
{
    public class AddModulePageVM : BaseVM, IPageVM
    {
        public AddModulePageVM(Group selectedGroup = null)
        {
            Persons = new ObservableCollection<CheckedListItem<Person>>();
            using (var _context = new ApplicationContext())
            {
                Courses = new ObservableCollection<Course>(_context.Courses.Include(x => x.Groups.Select(t => t.Modules)));
                var persons = _context.Students
                                .Include(x => x.Person)
                                .Include(x => x.Module.Group)
                                .Select(x => x.Person)
                                .Distinct();
                foreach(var person in persons)
                {
                    Persons.Add(new CheckedListItem<Person>(person));
                }
            }

            if (selectedGroup != null)
            {
                SelectedGroup = selectedGroup;
                SelectedCourse = selectedGroup.Course;
            }
            else
            {
                SelectedCourse = Courses.FirstOrDefault();
            }

            CalculateDateBounds();

            EventsManager.ObjectChangedEvent += (s, e) =>
            {
                if (e.ObjectChanged is Group || e.ObjectChanged is Module || e.ObjectChanged is Course || e.ObjectChanged is Student)
                {
                    int? id = SelectedGroup?.ID;
                    using (var _context = new ApplicationContext())
                    {
                        Persons.Clear();
                        Courses = new ObservableCollection<Course>(_context.Courses.Include(x => x.Groups.Select(t => t.Modules)));
                        var persons = _context.Students
                                        .Include(x => x.Person)
                                        .Include(x => x.Module.Group.Course)
                                        .Select(x => x.Person)
                                        .Distinct();
                        foreach (var person in persons)
                        {
                            Persons.Add(new CheckedListItem<Person>(person));
                        }
                        if (id != null)
                        {
                            SelectedGroup = _context.Groups.FirstOrDefault(x => x.ID == id);
                            SelectedCourse = SelectedGroup?.Course;
                            if (SelectedCourse != null)
                                Groups = new ObservableCollection<Group>(SelectedCourse.Groups);
                            else
                                SelectedCourse = Courses.FirstOrDefault();
                        }
                        else
                        {
                            SelectedCourse = Courses.FirstOrDefault();
                        }
                    }
                }
            };
        }

        private Course _SelectedCourse;
        public Course SelectedCourse
        {
            get
            {
                return _SelectedCourse;
            }
            set
            {
                _SelectedCourse = value;
                if (_SelectedCourse != null)
                {
                    Groups = new ObservableCollection<Group>(_SelectedCourse.Groups);
                    if (SelectedGroup == null)
                        SelectedGroup = Groups?.FirstOrDefault();
                }
                else
                {
                    Groups = new ObservableCollection<Group>();
                }
                OnPropertyChanged("SelectedCourse");
            }
        }

        private ObservableCollection<Course> _Courses;
        public ObservableCollection<Course> Courses
        {
            get
            {
                return _Courses;
            }
            set
            {
                _Courses = value;
                OnPropertyChanged("Courses");
            }
        }


        private ObservableCollection<Group> _Groups;
        public ObservableCollection<Group> Groups
        {
            get
            {
                return _Groups;
            }
            set
            {
                _Groups = value;
                OnPropertyChanged("Groups");
            }
        }

        private Group _SelectedGroup;
        public Group SelectedGroup
        {
            get
            {
                return _SelectedGroup;
            }
            set
            {
                _SelectedGroup = value;
                OnPropertyChanged("SelectedGroup");
                if (_SelectedGroup != null)
                {
                    if (_SelectedGroup.Modules.Count > 0)
                    {
                        ModuleNumber = _SelectedGroup.Modules.OrderBy(x => x.Number).Select(x => x.Number).Last() + 1;
                    }
                    else ModuleNumber = 1;
                }

            }
        }

        private ObservableCollection<CheckedListItem<Person>> _Persons;
        public ObservableCollection<CheckedListItem<Person>> Persons
        {
            get
            {
                return _Persons;
            }
            set
            {
                _Persons = value;
                OnPropertyChanged("Persons");
            }
        }

        private void CalculateDateBounds()
        {
            DateStart = DateTime.Now;
            DateEnd = DateStart.AddMonths(Constants.DefaultModuleLengthMonth);
        }

        private DateTime _DateStart;
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

        private DateTime _DateEnd;
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

        private int _ModuleNumber;
        public int ModuleNumber
        {
            get
            {
                return _ModuleNumber;
            }
            set
            {
                _ModuleNumber = value;
                OnPropertyChanged("ModuleNumber");
            }
        }

        

        private void AddModule()
        {
            using (var _context = new ApplicationContext())
            {
                var module = new Module()
                {
                    DateStart = DateStart,
                    DateEnd = DateEnd,
                    Group_ID = SelectedGroup.ID,
                    Number = ModuleNumber
                };
                var added = _context.Modules.Add(module);
                _context.SaveChanges();
                foreach(var person in Persons.Where(x => x.IsChecked))
                {
                    Student student = new Student()
                    {
                        DateStart = DateStart,
                        Person_ID = person.Item.ID,
                        Module_ID = added.ID
                    };
                    _context.Students.Add(student);
                    _context.Entry(student).State = EntityState.Added;
                }
                _context.SaveChanges();
                EventsManager.RaiseObjectChangedEvent(module);
            }
        }

        private ICommand _AddModuleCommand;
        public ICommand AddModuleCommand =>
            _AddModuleCommand ??
            (_AddModuleCommand = new RelayCommand(
                (obj) =>
                {
                    AddModule();
                }
                ));
    }
}