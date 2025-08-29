using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using Ukagaka;
using System.Windows.Media;
using System.Windows.Data;

namespace Cocoa.AppKit
{

    // Column definition class
    public class TableViewColumn
    {
        public string Identifier { get; set; }
        public string Title { get; set; }
        public string BindingPath { get; set; }
        public GridLength Width { get; set; } = new GridLength(1, GridUnitType.Star);

        public TableViewColumn(string identifier, string title, string bindingPath)
        {
            Identifier = identifier;
            Title = title;
            BindingPath = bindingPath;
        }
    }


    public class NSTableView:NSView
    {
         
        // Private fields
        private Grid _headerGrid;
        private ScrollViewer _scrollViewer;
        private StackPanel _rowsPanel;
        private List<NSTableColumn> _columns = new List<NSTableColumn>();
        private IEnumerable _itemsSource;
        private Type _itemType;
        private Dictionary<string, NSTableColumn> _columnMap = new Dictionary<string, NSTableColumn>();

        // Public properties
        public IEnumerable ItemsSource
        {
            get => _itemsSource;
            set
            {
                _itemsSource = value;
                UpdateItemsSource();
                OnPropertyChanged(nameof(ItemsSource));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Constructor
        public NSTableView()
        {
            // Create visual tree
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Main grid layout
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Header grid
            _headerGrid = new Grid();
            _headerGrid.Background = Brushes.LightGray;
            Grid.SetRow(_headerGrid, 0);
            mainGrid.Children.Add(_headerGrid);

            // Scroll viewer for rows
            _scrollViewer = new ScrollViewer();
            Grid.SetRow(_scrollViewer, 1);
            mainGrid.Children.Add(_scrollViewer);

            // Rows panel
            _rowsPanel = new StackPanel();
            _scrollViewer.Content = _rowsPanel;

            this.Content = mainGrid;
        }

        // Add a column to the table
        public void AddColumn(NSTableColumn column)
        {
            _columns.Add(column);
            _columnMap[column.Identifier] = column;
            UpdateColumns();
        }

        // Remove a column from the table
        public void RemoveColumn(string identifier)
        {
            if (_columnMap.TryGetValue(identifier, out var column))
            {
                _columns.Remove(column);
                _columnMap.Remove(identifier);
                UpdateColumns();
            }
        }

        // Clear all columns
        public void ClearColumns()
        {
            _columns.Clear();
            _columnMap.Clear();
            UpdateColumns();
        }

        // Update the column headers
        private void UpdateColumns()
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                _headerGrid.ColumnDefinitions.Clear();
                _headerGrid.Children.Clear();

                _headerGrid.ColumnDefinitions.Clear();
                _headerGrid.Children.Clear();

                foreach (var column in _columns)
                {
                    _headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = column.Width });

                    var header = new TextBlock
                    {
                        Text = column.Title,
                        Padding = new Thickness(5),
                        TextAlignment = TextAlignment.Left,
                        FontWeight = FontWeights.Bold
                    };

                    Grid.SetColumn(header, _columns.IndexOf(column));
                    _headerGrid.Children.Add(header);
                }
            });

        }

        // Update the rows when ItemsSource changes
        private void UpdateItemsSource()
        {
            _rowsPanel.Children.Clear();

            if (_itemsSource == null) return;

            // Get item type for binding
            var enumerator = _itemsSource.GetEnumerator();
            if (enumerator.MoveNext())
            {
                _itemType = enumerator.Current.GetType();
            }

            int rowIndex = 0;
            foreach (var item in _itemsSource)
            {
                var row = new Grid { Background = rowIndex % 2 == 0 ? Brushes.White : Brushes.WhiteSmoke };

                // Copy column definitions from header
                foreach (var colDef in _headerGrid.ColumnDefinitions)
                {
                    row.ColumnDefinitions.Add(new ColumnDefinition { Width = colDef.Width });
                }

                // Add cells
                foreach (var column in _columns)
                {
                    var content = new ContentControl();
                    content.ContentTemplate = CreateCellTemplate(column);
                    content.DataContext = item;
                    content.HorizontalAlignment = HorizontalAlignment.Stretch;
                    content.VerticalAlignment = VerticalAlignment.Stretch;
                    Grid.SetColumn(content, _columns.IndexOf(column));
                    row.Children.Add(content);
                }

                _rowsPanel.Children.Add(row);
                rowIndex++;
            }
        }

        // Create a data template for cell content
        private DataTemplate CreateCellTemplate(TableViewColumn column)
        {
            var factory = new FrameworkElementFactory(typeof(TextBlock));
            factory.SetBinding(TextBlock.TextProperty, new Binding(column.BindingPath));
            factory.SetValue(TextBlock.PaddingProperty, new Thickness(5));
            factory.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
            factory.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);

            var template = new DataTemplate { VisualTree = factory };
            return template;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public interface Delegate
        {



        }

        private Delegate _Delegate;

        public void SetDelegate(Delegate _Delegate)
        { 
            this._Delegate = _Delegate;
        }



        public class DataSource: Delegate
        {
            public void SetEnabled(bool value)
            {

            }

            public void SetTarGet(object obj)
            {
               
            }


        }



        public void SetDataSource(DataSource DataSource)
        {
             
        }

        internal NSTableColumn TableColumnWithIdentifier(string v)
        {
            if (_columnMap == null)
            {
                return null;
            }
             
            if (!_columnMap.ContainsKey(v))
            {
                return null;
            }

            return _columnMap[v];
           // throw new NotImplementedException();
        }

        internal void ReloadData()
        {
            UpdateColumns();

           // throw new NotImplementedException();
        }

        public void SetAutoSaveName(string name)
        {

        }

        internal void SetAutoSaveTableColumns(bool isAutoSave)
        {
            
        }

        internal void DeSelectAll(SCShellsList sCShellsList)
        {
            //throw new NotImplementedException();
        }

        internal void ScrollRowToVisible(int found_id)
        {
           // throw new NotImplementedException();
        }

        internal void DeselectAll(SCInstalledGhostsList sCInstalledGhostsList)
        {
           // throw new NotImplementedException();
        }

        internal int SelectedRow()
        {
            return 0;
        }

        internal int NumberOfRows()
        {

            return _columnMap.Count;
            //throw new NotImplementedException();
        }
    }
}
