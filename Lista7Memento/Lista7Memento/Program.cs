using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lista7Memento
{
    public class Distance
    {
        public static double calculate(Point x, Point y)
        {
            return Math.Sqrt(Math.Pow((x.X - y.X),2) + Math.Pow((x.Y - y.Y),2));
        }
    }
    
    public abstract class Shapes
    {
        public Point s;
        public float width;
        public float height;
        public virtual bool isInside(Point a)
        {
            return true;
        }
        public abstract Shapes Clone();
    }

    public class Circle : Shapes
    {
        public Circle(Point p, float width, float height)
        {
            this.s = p;
            this.width = width;
            this.height = height;
        }
        public override bool isInside(Point a)
        {
            return Distance.calculate(a, s) < 50;
        }

        public override Shapes Clone()
        {
            return (Shapes)this.MemberwiseClone();
        }
    }
    public class Rectangle : Shapes
    {
        public Rectangle(Point p, float width, float height)
        {
            this.s = p;
            this.width = width;
            this.height = height;
        }
        public override bool isInside(Point a)
        {
            return Distance.calculate(a, s) < 50 || Distance.calculate(a, new Point(s.X + 100, s.Y)) < 50;
        }
        public override Shapes Clone()
        {
            return (Shapes)this.MemberwiseClone();
        }
    }
    public class Square : Shapes
    {
        public Square(Point p, float width, float height)
        {
            this.s = p;
            this.width = width;
            this.height = height;
        }
        public override bool isInside(Point a)
        {
            return Distance.calculate(a, s) < 50;
        }
        public override Shapes Clone()
        {
            return (Shapes)this.MemberwiseClone();
        }
    }
    public enum Operation { None, Circle, Square, Rectangle, Move, Delete }
    public class OperationState
    {
        private Operation operation;
        private Shapes shapeAffected;
        private Point oldPosition;//for move
        private Point newPosition;//for move
        public Operation Operation
        {
            get { return operation; }
        }
        public Shapes ShapeAffected
        {
            get { return shapeAffected; }
        }
        public Point OldPosition{ 
            get { return oldPosition; }
        }
        public Point NewPosition
        {
            get { return newPosition; }
        }
        public OperationState(Operation operation,Shapes shape, Point oldPosition, Point newPosition)
        {
            this.operation = operation;
            this.shapeAffected = shape;
            this.oldPosition = oldPosition;
            this.newPosition = newPosition;
        }
    }
    public class Originator<T>
    {
        public Originator()
        {
        }
        private T _state;
        public T State
        {
            get { return _state; }
            set
            {
                _state = value;
                undoStates.Push(this.CreateMemento());
                redoStates.Clear();
            }
        }
        public Memento<T> CreateMemento()
        {
            Memento<T> memento = new Memento<T>();
            memento.SetState(this._state);
            return memento;
        }
        public void RestoreMemento(Memento<T> memento)
        {
            this._state = memento.GetState();
        }
        // część implementacji odpowiedzialna za Undo/Redo
        // górny element stosu undo to bieżący stan
        // "poprzedni" stan leży pod bieżącym
        Stack<Memento<T>> undoStates = new Stack<Memento<T>>();
        Stack<Memento<T>> redoStates = new Stack<Memento<T>>();
        public void Undo()
        {
            if (undoStates.Count > 1)
            {
                Memento<T> currentState = undoStates.Pop();
                redoStates.Push(currentState);
                Memento<T> previousState = undoStates.Peek();
                this.RestoreMemento(previousState);
            }
        }
        public void Redo()
        {
            if (redoStates.Count > 0)
            {
                Memento<T> futureState = redoStates.Pop();
                undoStates.Push(futureState);
                this.RestoreMemento(futureState);
            }
        }
    }
    public class Memento<T>
    {
        public T State { get; set; }
        public Memento()
        {
        }
        public T GetState()
        {
            return this.State;
        }
        public void SetState(T State)
        {
            this.State = State;
        }
    }
    public class ShapeManager : Form
    {
        private Originator<OperationState> originator;
        private MenuStrip menuStrip;
        private ToolStripMenuItem squareStrip;
        private ToolStripMenuItem rectangleStrip;
        private ToolStripMenuItem circleStrip;
        private ToolStripMenuItem moveStrip;
        private ToolStripMenuItem deleteStrip;
        private ToolStripMenuItem undoStrip;
        private ToolStripMenuItem redoStrip;
        private List<Shapes> shapes;
        private Square squarePrototype;
        private Circle circlePrototype;
        private Rectangle rectanglePrototype;
        private Operation currentOperationSelected;

        private bool startMove = false;
        private Point startPoint;
        public ShapeManager()
        {
            originator = new Originator<OperationState>();
            currentOperationSelected = Operation.None;
            shapes = new List<Shapes>();
            squarePrototype = new Square(new Point(0,0), 100, 100);
            circlePrototype = new Circle(new Point(0, 0), 100, 100);
            rectanglePrototype = new Rectangle(new Point(0, 0), 200, 100);

            menuStrip = new MenuStrip();
            squareStrip = new ToolStripMenuItem("Square");
            squareStrip.Click += squareStripClicked;
            rectangleStrip = new ToolStripMenuItem("Rectangle");
            rectangleStrip.Click += rectangleStripClicked;
            circleStrip = new ToolStripMenuItem("Circle");
            circleStrip.Click += circleStripClicked;
            moveStrip = new ToolStripMenuItem("Move");
            moveStrip.Click += moveStripClicked;
            deleteStrip = new ToolStripMenuItem("Delete");
            deleteStrip.Click += deleteStripClicked;
            undoStrip = new ToolStripMenuItem("Undo");
            undoStrip.Click += undoStripClicked;
            redoStrip = new ToolStripMenuItem("Redo");
            redoStrip.Click += redoStripClicked;

            menuStrip.Items.Add(squareStrip);
            menuStrip.Items.Add(rectangleStrip);
            menuStrip.Items.Add(circleStrip);
            menuStrip.Items.Add(moveStrip);
            menuStrip.Items.Add(deleteStrip);
            menuStrip.Items.Add(undoStrip);
            menuStrip.Items.Add(redoStrip);

            this.SuspendLayout();
            this.MouseUp += mouseUpWhenMove;
            this.MouseDown += mouseDownWhenMove;
            this.Click += doCurrentOperation;
            this.ClientSize = new Size(650, 520);
            this.Controls.Add(menuStrip);
            
            this.ResumeLayout();
        }
        private void squareStripClicked(object sender, EventArgs e)
        {
            currentOperationSelected = Operation.Square;
        }
        private void rectangleStripClicked(object sender, EventArgs e)
        {
            currentOperationSelected = Operation.Rectangle;
        }
        private void circleStripClicked(object sender, EventArgs e)
        {
            currentOperationSelected = Operation.Circle;
        }
        private void moveStripClicked(object sender, EventArgs e)
        {
            currentOperationSelected = Operation.Move;
        }
        private void deleteStripClicked(object sender, EventArgs e)
        {
            currentOperationSelected = Operation.Delete;
        }
        private void undoStripClicked(object sender, EventArgs e)
        {
            if(originator.State != null)
            {
                switch (originator.State.Operation)
                {
                    case Operation.Square:
                        shapes.Remove(originator.State.ShapeAffected);
                        break;
                    case Operation.Circle:
                        shapes.Remove(originator.State.ShapeAffected);
                        break;
                    case Operation.Rectangle:
                        shapes.Remove(originator.State.ShapeAffected);
                        break;
                    case Operation.Move:
                        originator.State.ShapeAffected.s = originator.State.OldPosition;
                        break;
                    case Operation.Delete:
                        shapes.Add(originator.State.ShapeAffected);
                        break;
                }
            }
            originator.Undo();
            drawEveryThing();
        }
        private void redoStripClicked(object sender, EventArgs e)
        {
            originator.Redo();
            if (originator.State != null)
            {
                switch (originator.State.Operation)
                {
                    case Operation.Square:
                        shapes.Add(originator.State.ShapeAffected);
                        break;
                    case Operation.Circle:
                        shapes.Add(originator.State.ShapeAffected);
                        break;
                    case Operation.Rectangle:
                        shapes.Add(originator.State.ShapeAffected);
                        break;
                    case Operation.Move:
                        originator.State.ShapeAffected.s = originator.State.NewPosition;
                        break;
                    case Operation.Delete:
                        shapes.Remove(originator.State.ShapeAffected);
                        break;
                }
            }
            
            drawEveryThing();
        }
        private void doCurrentOperation(object sender, EventArgs e)
        {
            if (currentOperationSelected == Operation.Move) return;
            
            Point cp = PointToClient(Cursor.Position);

            switch (currentOperationSelected)
            {
                case Operation.Square:
                    Square temp1 = (Square)squarePrototype.Clone();
                    temp1.s = cp;
                    shapes.Add(temp1);
                    originator.State = new OperationState(Operation.Square, temp1, new Point(0,0), new Point(0, 0));
                    break;
                case Operation.Circle:
                    Circle temp2 = (Circle)circlePrototype.Clone();
                    temp2.s = cp;
                    shapes.Add(temp2);
                    originator.State = new OperationState(Operation.Circle, temp2, new Point(0, 0), new Point(0, 0));
                    break;
                case Operation.Rectangle:
                    Rectangle temp3 = (Rectangle)rectanglePrototype.Clone();
                    temp3.s = cp;
                    shapes.Add(temp3);
                    originator.State = new OperationState(Operation.Rectangle, temp3, new Point(0, 0), new Point(0, 0));
                    break;
                case Operation.Move:
                    break;
                case Operation.Delete:
                    Shapes find = findFirstShape(cp);
                    if(find != null)
                    {
                        shapes.Remove(find);
                        originator.State = new OperationState(Operation.Delete, find, new Point(0, 0), new Point(0, 0));
                    }
                    break;
            }
            drawEveryThing();           
        }
        private void drawEveryThing()
        {
            Graphics g = this.CreateGraphics();
            g.Clear(Color.White);
            Pen p = new Pen(Color.Black, 3);
            foreach (Shapes sh in shapes)
            {
                if (sh.GetType() == typeof(Circle))
                {
                    p = new Pen(Color.Green, 3);
                    g.DrawEllipse(p, sh.s.X - 50, sh.s.Y - 50, sh.width, sh.height);
                }
                else if (sh.GetType() == typeof(Square))
                {
                    p = new Pen(Color.Red, 3);
                    g.DrawRectangle(p, sh.s.X - 50, sh.s.Y - 50, sh.width, sh.height);
                }
                else if (sh.GetType() == typeof(Rectangle))
                {
                    p = new Pen(Color.Blue, 3);
                    g.DrawRectangle(p, sh.s.X - 50, sh.s.Y - 50, sh.width, sh.height);
                }
            }
        }

        private void mouseUpWhenMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (startMove && currentOperationSelected == Operation.Move)
            {
                startMove = false;
                Point mouseUpLocation = new Point(e.X, e.Y);
                Shapes sh = findFirstShape(startPoint);
                if (sh != null)
                {
                    Point oldpoint = new Point(sh.s.X, sh.s.Y);
                    Point newpoint = new Point(mouseUpLocation.X, mouseUpLocation.Y);
                    sh.s.X = mouseUpLocation.X;
                    sh.s.Y = mouseUpLocation.Y;
                    originator.State = new OperationState(Operation.Move, sh, oldpoint, newpoint);
                }

            }        
            drawEveryThing();
        }
        private void mouseDownWhenMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!startMove)
            {
                Point mouseDownLocation = new Point(e.X, e.Y);
                startMove = true;
                startPoint = mouseDownLocation;
            }
        }
        private Shapes findFirstShape(Point x)
        {
            foreach(Shapes s in shapes)
            {
                if (s.isInside(x))
                {
                    return s;
                }
            }
            return null;
        }
    }
    public class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ShapeManager());
        }
    }
}
