using System.Collections.Generic;
using System.Windows.Input;
using NetworkService.Models;
using NetworkService.MVVM;
using NetworkService.Services;

namespace NetworkService.ViewModels
{

    public enum InputField
    {
        None,
        Id,
        Name
    }


    public class AddEntityViewModel : PageViewModel
    {
        private string _idText = string.Empty;
        private string _name = string.Empty;
        private EntityType _selectedType;

        private string _idError;
        private string _nameError;
        private string _typeError;

        private InputField _activeField = InputField.Id;

        public AddEntityViewModel(IAppController controller) : base(controller)
        {
            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(controller.NavigateToEntities);
            KeyCommand = new RelayCommand(p => PressKey(p as string));
        }

        public override string Title => "Dodaj entitet";

        public IReadOnlyList<EntityType> Types => EntityTypeCatalog.All;

        public string IdText
        {
            get => _idText;
            set
            {
                if (SetProperty(ref _idText, value))
                {
                    ValidateId();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    ValidateName();
                }
            }
        }

        public EntityType SelectedType
        {
            get => _selectedType;
            set
            {
                if (SetProperty(ref _selectedType, value))
                {
                    ValidateType();
                }
            }
        }

        public string IdError
        {
            get => _idError;
            private set
            {
                if (SetProperty(ref _idError, value))
                {
                    OnPropertyChanged(nameof(HasIdError));
                }
            }
        }

        public string NameError
        {
            get => _nameError;
            private set
            {
                if (SetProperty(ref _nameError, value))
                {
                    OnPropertyChanged(nameof(HasNameError));
                }
            }
        }

        public string TypeError
        {
            get => _typeError;
            private set
            {
                if (SetProperty(ref _typeError, value))
                {
                    OnPropertyChanged(nameof(HasTypeError));
                }
            }
        }

        public bool HasIdError => !string.IsNullOrEmpty(IdError);
        public bool HasNameError => !string.IsNullOrEmpty(NameError);
        public bool HasTypeError => !string.IsNullOrEmpty(TypeError);

  
        public InputField ActiveField
        {
            get => _activeField;
            set => SetProperty(ref _activeField, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand KeyCommand { get; }


        private void PressKey(string key)
        {
            if (key == null)
            {
                return;
            }

            if (ActiveField == InputField.Id)
            {
                if (key == "BACK")
                {
                    if (IdText.Length > 0)
                    {
                        IdText = IdText.Substring(0, IdText.Length - 1);
                    }
                }
                else if (key.Length == 1 && char.IsDigit(key[0]))
                {

                    IdText += key;
                }
            }
            else if (ActiveField == InputField.Name)
            {
                if (key == "BACK")
                {
                    if (Name.Length > 0)
                    {
                        Name = Name.Substring(0, Name.Length - 1);
                    }
                }
                else if (key == "SPACE")
                {
                    Name += " ";
                }
                else
                {
                    Name += key;
                }
            }
        }

        private void Save()
        {

            ValidateId();
            ValidateName();
            ValidateType();

            if (HasIdError || HasNameError || HasTypeError)
            {
                return;
            }

            var entity = new Entity
            {
                Id = int.Parse(IdText.Trim()),
                Name = Name.Trim(),
                Type = SelectedType
            };

            Controller.AddEntity(entity);
            Controller.NavigateToEntities();
        }

        private void ValidateId()
        {
            string text = (IdText ?? string.Empty).Trim();
            if (text.Length == 0)
            {
                IdError = "ID je obavezan.";
            }
            else if (!int.TryParse(text, out int id))
            {
                IdError = "ID mora biti jedinstven ceo broj!";
            }
            else if (!Controller.IsIdUnique(id))
            {
                IdError = "Entitet sa ovim ID-jem već postoji.";
            }
            else
            {
                IdError = null;
            }
        }

        private void ValidateName()
        {
            NameError = string.IsNullOrWhiteSpace(Name) ? "Naziv je obavezan." : null;
        }

        private void ValidateType()
        {
            TypeError = SelectedType == null ? "Izaberite tip entiteta." : null;
        }
    }
}
