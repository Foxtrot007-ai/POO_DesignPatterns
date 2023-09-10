using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace Register
{   
    public class Person
    {
        private String name;
        public String Name
        { 
            get { return name; }
            set { name = value; } 
        }
        private String lastName;
        public String LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        private String dateOfBirth;
        public String DateOfBirth
        {
            get { return dateOfBirth; }
            set { dateOfBirth = value; }
        }

        private String address;
        public String Address
        {
            get { return address; }
            set { address = value; }
        }

        public Person(string name, string lastName, string dateOfBirth, string address)
        {
            this.name = name;
            this.lastName = lastName;
            this.dateOfBirth = dateOfBirth;
            this.address = address;
        }
        public bool compareWithOtherPerson(Person p)
        {
            return (Name == p.Name)
                    && (LastName == p.LastName)
                    && (DateOfBirth == p.DateOfBirth)
                    && (Address == p.Address);
        }
        public override string ToString()
        {
            return name + " " + lastName;
        }

    }
    public class ModalDialog : Form
    {
        public TextBox textBox1;
        public TextBox textBox2;
        public TextBox textBox3;
        public TextBox textBox4;
        public Label label1;
        public Label label2;
        public Label label3;
        public Label label4;
        public Button button;
        public ModalDialog()
        {

            
            label1 = new Label();
            label1.Size = new Size(100, 20);
            label1.Location = new Point(50, 5);
            label1.Text = "Name";
            textBox1 = new TextBox();
            textBox1.Size = new Size(100, 20);
            textBox1.Location = new Point(50, 25);

            label2 = new Label();
            label2.Size = new Size(100, 20);
            label2.Location = new Point(200, 5);
            label2.Text = "Last Name";
            textBox2 = new TextBox();
            textBox2.Size = new Size(100, 20);
            textBox2.Location = new Point(200, 25);

            label3 = new Label();
            label3.Size = new Size(100, 20);
            label3.Location = new Point(350, 5);
            label3.Text = "Birth of date";
            textBox3 = new TextBox();
            textBox3.Size = new Size(100, 20);
            textBox3.Location = new Point(350, 25);

            label4 = new Label();
            label4.Size = new Size(150, 20);
            label4.Location = new Point(500, 5);
            label4.Text = "Address";
            textBox4 = new TextBox();
            textBox4.Size = new Size(150, 20);
            textBox4.Location = new Point(500, 25);

            button = new Button();
            button.Size = new Size(100, 20);
            button.Text = "Ok";    
            button.Location = new Point(300, 55);
            button.Click += buttonSave;
            this.SuspendLayout();
            this.ClientSize = new Size(700, 100);
            this.Controls.Add(label1);
            this.Controls.Add(label2);
            this.Controls.Add(label3);
            this.Controls.Add(label4);
            this.Controls.Add(textBox1);
            this.Controls.Add(textBox2);
            this.Controls.Add(textBox3);
            this.Controls.Add(textBox4);
            this.Controls.Add(button);
            this.ResumeLayout();
        }
        private void buttonSave(object sender, System.EventArgs e)
        {
            this.Close();
        }
    }
    public interface ISubscriber<T>
    {
        void HandleNotification(T notification);
    }
    public interface IEventAggregator
    {
        void AddSubscriber<T>(ISubscriber<T> subscriber);
        void RemoveSubscriber<T>(ISubscriber<T> subscriber);
        void Publish<T>(T Event);
    }
    public class EventAggregator : IEventAggregator
    {
        Dictionary<Type, List<object>> _subscribers =
            new Dictionary<Type, List<object>>();

        public void AddSubscriber<T>(ISubscriber<T> subscriber)
        {
            if (!_subscribers.ContainsKey(typeof(T)))
                _subscribers.Add(typeof(T), new List<object>());

            _subscribers[typeof(T)].Add(subscriber);
        }
        public void RemoveSubscriber<T>(ISubscriber<T> subscriber)
        {
            if (!_subscribers.ContainsKey(typeof(T)))
                _subscribers[typeof(T)].Remove(subscriber);
        }
        public void Publish<T>(T Event)
        {
            if(_subscribers.ContainsKey(typeof(T)))
                foreach(ISubscriber<T> subscriber in _subscribers[typeof(T)].OfType<ISubscriber<T>>())
                    subscriber.HandleNotification(Event);
        }
    }
    public class ShowPersonNotification
    {
        public String type;
        public Person person;
        public ShowPersonNotification(String t, Person p)
        {
            type = t;
            person = p;
        }
    }
    public class ShowListNotification
    {
        public String type;
        public List<Person> persons;
        public ShowListNotification(String t, List<Person> persons)
        {
            this.type = t;
            this.persons = persons;
        }
    }
    public class UpdateListNotification
    {
        public String type;
        public Person person;
        public UpdateListNotification(String t, Person p)
        {
            type = t;
            person = p;
        }
    }
    public class UpdatePersonNotification
    {
        public String type;
        public Person oldperson;
        public Person newperson;
        public UpdatePersonNotification(String t, Person newinfo, Person oldinfo)
        {
            type = t;
            oldperson = oldinfo;
            newperson = newinfo;
        }
    }
    public class RegisterTree : TreeView, ISubscriber<UpdateListNotification>, ISubscriber<UpdatePersonNotification>
    {
        public List<Person> students = new List<Person>();
        public List<Person> lecturers = new List<Person>();
        private EventAggregator eventAggregator;

        public RegisterTree(EventAggregator e)
        {
            eventAggregator = e;
            students.Add(new Person("Dominik", "Baziuk", "14-07-2001", "Wyspiańskiego 53/32"));
            students.Add(new Person("Andrzej", "Baziuk", "14-07-1977", "Wyspiańskiego 53/32"));
            students.Add(new Person("Jan", "Baziuk", "14-07-1255", "Wyspiańskiego 53/32"));
            lecturers.Add(new Person("Someone", "Someone", "14-07-2124", "Somewhere"));
            lecturers.Add(new Person("Jan", "Kowalski", "14-07-2222", "Somewhere"));
            lecturers.Add(new Person("Piotr", "Nowak", "14-07-1245", "Somewhere"));
        }
        public void FirstInitialize()
        {
            this.Location = new Point(0, 0);
            this.Size = new Size(200, 500);
            this.CheckBoxes = false;
            this.AfterSelect += SelectNodes;
            eventAggregator.AddSubscriber<UpdateListNotification>(this);
            UpdateNodes();
        }
        public void SelectNodes(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            //Node
            if(e.Action != TreeViewAction.ByMouse)
            {
                return;
            }
            String option = e.Node.Text;
            if (option == "students" || option == "lecturers")
            {
                eventAggregator.RemoveSubscriber<UpdatePersonNotification>(this);
                eventAggregator.AddSubscriber<UpdateListNotification>(this);
                ShowListNotification notification;
                if (option == "students")
                {
                    notification = new ShowListNotification(option, students);
                }
                else
                {
                    notification = new ShowListNotification(option, lecturers);
                }
                eventAggregator.Publish<ShowListNotification>(notification);
            }
            else // persons
            {
                ShowPersonNotification notification;
                String type = "students";
                Person p = students.Find(x => x.ToString() == option);
                if(p == null)
                {
                    type = "lecturers";
                    p = lecturers.Find(x => x.ToString() == option);
                }

                if(p != null)
                {
                    eventAggregator.RemoveSubscriber<UpdateListNotification>(this);
                    eventAggregator.AddSubscriber<UpdatePersonNotification>(this);
                    notification = new ShowPersonNotification(type, p);
                    eventAggregator.Publish<ShowPersonNotification>(notification);
                }
            }
        }
        public void HandleNotification(UpdatePersonNotification notification)
        {
            if (notification.type == "students")
            {
                int i = students.FindIndex(x => x.compareWithOtherPerson(notification.oldperson));
                if (i == -1) return;
                students[i] = notification.newperson;
            }
            else if (notification.type == "lecturers")
            {
                int i = lecturers.FindIndex(x => x.compareWithOtherPerson(notification.oldperson));
                if (i == -1) return;
                lecturers[i] = notification.newperson;
            }
            this.UpdateNodes();
        }
        public void HandleNotification(UpdateListNotification notification)
        {
            if(notification.type == "students")
            {
                if(students.FindIndex(x => x.compareWithOtherPerson(notification.person)) == -1)
                    students.Add(notification.person);
            }else if (notification.type == "lecturers")
            {
                if (lecturers.FindIndex(x => x.compareWithOtherPerson(notification.person)) == -1)
                    lecturers.Add(notification.person);
            }
            this.UpdateNodes();
        }
        public void UpdateNodes()
        {
            this.BeginUpdate();

            this.Nodes.Clear();

            this.Nodes.Add("students");
            foreach (Person p in students)
                this.Nodes[0].Nodes.Add(p.ToString());

            this.Nodes.Add("lecturers");
            foreach (Person p in lecturers)
                this.Nodes[1].Nodes.Add(p.ToString());

            this.EndUpdate();
        }
    }
    public enum PersonsViewerMode {None, Adding, Editing }
    public class PersonsViewer : DataGridView, ISubscriber<ShowListNotification>, ISubscriber<ShowPersonNotification>
    {
        private ModalDialog testDialog;
        private Button button;
        private PersonsViewerMode mode;
        private EventAggregator eventAggregator;
        private String currentType;
        private Person currentPerson;
        public PersonsViewer(EventAggregator e)
        {
            eventAggregator = e;
        }
        public void FirstInitialize()
        {
            testDialog = new ModalDialog();
            button = new Button();
            button.Location = new Point(175, 400);
            button.Size = new Size(120, 30);
            button.Text = "Accept";
            button.Click += new EventHandler(this.ClickedAccept);

            this.Location = new Point(200, 0);
            this.Size = new Size(450, 500);
            this.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;

            this.Controls.Add(button);
            this.ColumnCount = 4;
            this.Columns[0].Name = "Name";
            this.Columns[1].Name = "Last name";
            this.Columns[2].Name = "Date of birth";
            this.Columns[3].Name = "Address";
            this.Click += ClickedAdding;

            eventAggregator.AddSubscriber<ShowListNotification>(this);
            eventAggregator.AddSubscriber<ShowPersonNotification>(this);
            mode = PersonsViewerMode.None;
            currentType = "";
        }
        public void Populate(List<Person> persons) 
        {
            Rows.Clear();
            for(int i = 0; i < persons.Count; i++)
            {
                string[] row = { persons[i].Name, persons[i].LastName, persons[i].DateOfBirth, persons[i].Address };
                this.Rows.Add(row);
            }
        }

        public void SinglePopulate(Person p)
        {
            Rows.Clear();
            string[] row = { p.Name, p.LastName, p.DateOfBirth, p.Address };
            this.Rows.Add(row);
        }
        public void ClickedAdding(object sender, System.EventArgs e)
        {
            this.testDialog.ShowDialog();
        }

        public Person GetPersonFromDialog()
        {
            return new Person(testDialog.textBox1.Text,
                              testDialog.textBox2.Text,
                              testDialog.textBox3.Text,
                              testDialog.textBox4.Text);
        }
        public void ClickedAccept(object sender, System.EventArgs e)
        {
            if (mode == PersonsViewerMode.Adding)
            {
                UpdateListNotification notification = new UpdateListNotification(currentType, GetPersonFromDialog());
                eventAggregator.Publish<UpdateListNotification>(notification);
            }
            else if(mode == PersonsViewerMode.Editing)
            {
                UpdatePersonNotification notification = new UpdatePersonNotification(currentType, GetPersonFromDialog(), currentPerson);
                eventAggregator.Publish<UpdatePersonNotification>(notification);
            }
        }
        public void HandleNotification(ShowPersonNotification notification)
        { 
            mode = PersonsViewerMode.Editing;
            currentType = notification.type;
            currentPerson = notification.person;
            SinglePopulate(notification.person);
        }
        public void HandleNotification(ShowListNotification notification)
        {
            mode = PersonsViewerMode.Adding;
            currentType = notification.type;
            currentPerson = null;
            Populate(notification.persons);
        }
    }
    public class Register : Form
    {
        private RegisterTree tree;
        private PersonsViewer adder;
        private EventAggregator eventAggregator;
        public Register()
        {
            eventAggregator = new EventAggregator();
            tree = new RegisterTree(eventAggregator);
            adder = new PersonsViewer(eventAggregator);
            
            this.SuspendLayout();
            tree.FirstInitialize();
            adder.FirstInitialize();
            this.ClientSize = new Size(650, 520);
            this.Controls.Add(tree);
            this.Controls.Add(adder);
            this.ResumeLayout();
        }
    }

    public class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.Run(new Register());
        }
    }
}
