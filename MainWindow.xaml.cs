using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;  
using System.Text.Json;
using System.Collections.ObjectModel;

// TODO> fazer poder escolher props tipo "material -> madeira"
// Problemas :
    // nao ta rolando colocar mais de um material
    // Os materiais tao como string, nao uma ligacao pra coisa, resolver


namespace wpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        public App app;
    
        public WindowController Controller = new WindowController();

        
        public MainWindow(App app)
        {
            InitializeComponent();
            this.app = app;
            // TODO: Move this
            EdgeTypeSelector.Items.Add("IS_A");
            EdgeTypeSelector.Items.Add("IS_AT");
            EdgeTypeSelector.Items.Add("BELONGS_TO");
            EdgeTypeSelector.Items.Add("HAS");
            EdgeTypeSelector.Items.Add("CAUSES");
            EdgeTypeSelector.Items.Add("INSTANCE_OF");
            EdgeTypeSelector.Items.Add("HAS_PROP");
            EdgeTypeSelector.Items.Add("P_ACTION"); // Possible action
            EdgeTypeSelector.Items.Add("ABLE_ACTION"); // Able to do action

            // Flags
            CheckBox chb = new CheckBox();  
            chb.Content = "instantiable";
            chb.IsChecked = false;
            flagBox.Items.Add(chb);

            this.DataContext = Controller;
            lbUsers.ItemsSource = Controller.brothers;
            lbEdge.ItemsSource = Controller.incoming;
            lbOutNodes.ItemsSource = Controller.parents;
            lbOutEdges.ItemsSource = Controller.outcoming;
            Controller.mainWindow = this;
        }

        public void LogStuff(string msg) {
            lbResult.Items.Add(msg);
        }
        private void btnLoad_Click(object sender, RoutedEventArgs e)
		{
            try {
                var graph = JsonWorker.LoadDB();
                lbResult.Items.Add("Loaded : " + graph.GetNumEdges() + " edges.");
			    lbResult.Items.Add("Loaded : " + graph.GetNumNodes() + " nodes.");
                this.app.Graph = graph;
                ComboBox1.Items.Clear();
                ComboBox2.Items.Clear();
                NodeSelector.Items.Clear();
                foreach( var item in graph.NodesCollection ) {
                    ComboBox1.Items.Add(new KeyValuePair<string, Node>(item.Value.Label, item.Value));  
                    ComboBox2.Items.Add(new KeyValuePair<string, Node>(item.Value.Label, item.Value));  
                    NodeSelector.Items.Add(new KeyValuePair<string, Node>(item.Value.Label, item.Value));  
                }
                foreach( var item in graph.EdgesCollection) {
                    lbResult.Items.Add(
                        (graph.NodesCollection.GetValueOrDefault(item.Value.From)).Label +
                        " " +
                        item.Value.Label + 
                        " " + 
                        graph.NodesCollection.GetValueOrDefault(item.Value.To).Label
                    );
                }
            } catch ( Exception exc) {
                 lbResult.Items.Add("ERROR : " + exc.ToString());
            }
		}

        private void btnClearScreen_Click(object sender, RoutedEventArgs e)
		{
            lbResult.Items.Clear();
		}

        private void btnSave_Click(object sender, RoutedEventArgs e)
		{
            try {
                JsonWorker.SaveDB(this.app.Graph);
                lbResult.Items.Add("Saved To JSON.");
            } catch ( Exception exc) {
                lbResult.Items.Add("ERROR : " + exc.ToString());
            }
		}

        private void AddButton_Click(object sender, RoutedEventArgs e)  
        {  
            var newNode = new Node(TextBox1.Text);
            this.app.Graph.NodesCollection.Add(newNode.ID, newNode);
            ComboBox1.Items.Add(new KeyValuePair<string, Node>(TextBox1.Text, newNode));  
            ComboBox2.Items.Add(new KeyValuePair<string, Node>(TextBox1.Text, newNode));
            NodeSelector.Items.Add(new KeyValuePair<string, Node>(TextBox1.Text, newNode));   
        }

        private void createInstance_Click(object sender, RoutedEventArgs e)  
        {  
            if (TextBox1.Text == ""){
                lbResult.Items.Add("Can't create instance with no label!");
                return;  
            } 
            var parent = ((KeyValuePair<string, Node>)NodeSelector.SelectedValue).Value;
            // var inst = new Instance(TextBox1.Text, parent, this.app.Graph);
            // Falta selecionar e colocar na instancia as props.
            // lbResult.Items.Add("Creating Instance : " + inst);
            // var props = inst.GetProperties(this.app.Graph);

            
            printObj(this.Controller.InstanceData, "");
            
        }

        private void printObj(object obj, string indent) {
            if(obj is Dictionary<string, object>) {
                foreach(var i in (Dictionary<string, object>)obj) {
                    lbResult.Items.Add(indent + i.Key + ": ");
                    this.printObj(i.Value, indent + " ");
                }
            } else {
                lbResult.Items.Add(indent + obj);
            }
        }

        private void NodeSelector_Change(object sender, RoutedEventArgs e){
            try{
                var n1 = ((KeyValuePair<string, Node>)NodeSelector.SelectedValue).Value;
                this.Controller.SelectedNode = n1;
                var inEdges = this.app.Graph.GetEdges(n1.Incoming);
                var outEdges = this.app.Graph.GetEdges(n1.Outcoming);
                List<string>inNodesID = new List<string>();
                List<string>outNodesID = new List<string>();
                this.Controller.incoming.Clear();
                this.Controller.brothers.Clear();
                this.Controller.outcoming.Clear();
                this.Controller.parents.Clear();
                lbProps.Items.Clear();

                foreach( var item in inEdges) {
                    inNodesID.Add(item.From);
                    this.Controller.incoming.Add(item);
                }
                foreach( var item in outEdges) {
                    outNodesID.Add(item.To);
                    this.Controller.outcoming.Add(item);
                }
                var inNodes = this.app.Graph.GetNodes(inNodesID);
                var outNodes = this.app.Graph.GetNodes(outNodesID);
                foreach( var nn in inNodes) {
                    this.Controller.brothers.Add(nn);
                }
                foreach( var nn in outNodes) {
                    this.Controller.parents.Add(nn);
                }
                var props = n1.GetProperties(this.app.Graph);
                foreach(var prop in props ) {
                    this.LogStuff(prop.Label);
                    this.LogStuff(prop.AsProperty);
                    lbProps.Items.Add(prop.Label);
                }
                instanceWrapper.Children.Clear();
                foreach(var prop in props ) {
                    var propHolder = new StackPanel();
                    propHolder.Margin = new Thickness(5);
                    var propLabel = new Label();
                    var propListOfValues = new ListBox();
                    propLabel.Content = prop.Label;
                    propHolder.Children.Add(propLabel);
                    propHolder.Children.Add(propListOfValues);
                    instanceWrapper.Children.Add(propHolder);
                    List<string>inEdgesID = new List<string>();
                    foreach( var item in prop.Incoming) {
                        inEdgesID.Add(item);
                    }
                    var inEdges2 = this.app.Graph.GetEdges(inEdgesID);
                    foreach( var item in inEdges2) {
                        // Get possible values of prop
                        if(item.Label == "IS_A"){
                            var propValue = this.app.Graph.NodesCollection.GetValueOrDefault(item.From);
                            // Add the handlers of the events of change to change the instance to be created
                            var dataType = propValue.Data.GetValueOrDefault("Type");
                            var hStack = new WrapPanel();
                            var thingLabel = new Label();
                            var thingCheckbox = new CheckBox();
                            UIElement thingBox = null;
                            thingCheckbox.Uid = prop.Label + ":" + propValue.Label;
                            // Oh god why
                            thingCheckbox.Click += (object sender, RoutedEventArgs e) => GetHandlerFunc(thingBox, thingCheckbox)(sender, e);
                            thingCheckbox.VerticalAlignment = VerticalAlignment.Center;
                            thingLabel.Content = propValue.Label;
                            hStack.Children.Add(thingCheckbox);
                            hStack.Children.Add(thingLabel);
                            if(dataType != null && "number" == ((JsonElement)dataType).GetString()){
                                thingBox = new TextBox();
                                ((TextBox)thingBox).Width = 60;
                                ((TextBox)thingBox).Text = "";
                                hStack.Children.Add(thingBox);
                                
                            }
                            propListOfValues.Items.Add(hStack);
                        }
                    }
                }
            } catch ( Exception exc) {
                lbResult.Items.Add("ERROR : " + exc.ToString());
            }
        }

        private Action<object, RoutedEventArgs> GetHandlerFunc(UIElement tb, CheckBox cb){

            return (object sender, RoutedEventArgs e) => {
                var senderAsCheckBox = (CheckBox)sender;
                var finalValue = "";
                var isEnum = false;
                try {
                    var tbAsTextBox = (TextBox)tb;
                    finalValue = tbAsTextBox.Text;
                } catch( Exception ){
                    isEnum = true;
                }

                if(senderAsCheckBox.IsChecked ?? false ){
                    lbResult.Items.Add("Instance have [ " + senderAsCheckBox.Uid + " ] property with [ " + finalValue + " ] value!");
                    string[] levels = senderAsCheckBox.Uid.Split(":");
                    lbResult.Items.Add(levels);
                    var mostInternalDic = this.Controller.InstanceData;
                    // generating json based on UI
                    try {
                        for (int i = 0 ; i < levels.Length; i ++) {
                            lbResult.Items.Add(levels[i]);
                            if(mostInternalDic.GetValueOrDefault(levels[i]) != null){
                                var currentV = mostInternalDic.GetValueOrDefault(levels[i]);
                                if(currentV is Dictionary<string, object>) {
                                    mostInternalDic = (Dictionary<string, object>)currentV;
                                }
                                continue;
                            }
                            if(!isEnum){
                                if ( i != levels.Length - 1 ) {
                                    var temp = new Dictionary<string, object>();
                                    mostInternalDic.Add(levels[i], temp);
                                    mostInternalDic = temp;
                                    continue;
                                }
                                mostInternalDic.Add(levels[i], finalValue);
                            }else if(i > 0){
                                mostInternalDic.Add(levels[i-1], levels[i]);
                            }
                        }
                        // this.Controller.InstanceData.Add(senderAsCheckBox.Uid, tb.Text)
                    }catch(Exception exc) {
                        this.LogStuff(exc.Message);
                        this.LogStuff(exc.StackTrace);
                    }
                }
            };
        }

        private void LinkNodes_Click(object sender, RoutedEventArgs e)  
        {  
         try {
            var n1 = ((KeyValuePair<string, Node>)ComboBox1.SelectedValue).Value;
            var n2 = ((KeyValuePair<string, Node>)ComboBox2.SelectedValue).Value;
            var link = (string)EdgeTypeSelector.SelectedValue;

            var newEdge = new Edge(n1, n2, link);
            this.app.Graph.EdgesCollection.Add(newEdge.ID, newEdge);

            } catch ( Exception exc) {
                 lbResult.Items.Add("ERROR : " + exc.ToString());
            }
        } 
    }
    public class WindowController : INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Node> brothers = new ObservableCollection<Node>();
        public ObservableCollection<Node> parents = new ObservableCollection<Node>();
        public ObservableCollection<Edge> incoming = new ObservableCollection<Edge>();
        public ObservableCollection<Edge> outcoming = new ObservableCollection<Edge>();
        public MainWindow mainWindow;

        public Dictionary<string, Object> InstanceData = new Dictionary<string, Object>();
        private Node _selectedNode;
        public Node SelectedNode
        {  
            get  
            {  
                return _selectedNode;  
            }  

            set
            {  
                if (value != _selectedNode)  
                {  
                    _selectedNode = value;
                    NotifyPropertyChanged("SelectedNode");
                }  
            }  
        }  

      private void NotifyPropertyChanged(string propertyName = "")  
        {  
            if(this.PropertyChanged == null) this.mainWindow.LogStuff("ERROR: PropertyChanged is null!");
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void HandlePropertyChange(string propName, Object value) {
            if(value != null) {
                this.InstanceData.Add(propName, value);
                return;
            }
            this.InstanceData.Remove(propName);
        }   
    }
}
