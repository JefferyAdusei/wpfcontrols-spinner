using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Spyder.Controls.Spinner
{
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_IncreaseButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_DecreaseButton", Type = typeof(RepeatButton))]
    public class IntegerSpinner : Control
    {
        #region Private Constants

        /// <summary>
        /// Part name for text box
        /// </summary>
        private const string TextBoxName = "PART_TextBox";

        /// <summary>
        /// Part name for increase button.
        /// </summary>
        private const string IncreaseButtonName = "PART_IncreaseButton";

        /// <summary>
        /// Part name for decrease button.
        /// </summary>
        private const string DecreaseButtonName = "PART_DecreaseButton";

        #endregion

        #region Private Fields

        /// <summary>
        /// Gets a copy of the current culture for modifications.
        /// </summary>
        private readonly CultureInfo _culture;

        #endregion

        #region Field Commands

        /// <summary>
        /// <see cref="RoutedUICommand"/> that executes the minor increase function.
        /// </summary>
        private readonly RoutedUICommand _minorIncreaseValueCommand =
            new("MinorIncreaseValue", "MinorIncreaseValue", typeof(IntegerSpinner));

        /// <summary>
        /// <see cref="RoutedUICommand"/> that executes the minor decrease function.
        /// </summary>
        private readonly RoutedUICommand _minorDecreaseValueCommand =
            new("MinorDecreaseValue", "MinorDecreaseValue", typeof(IntegerSpinner));

        /// <summary>
        /// <see cref="RoutedUICommand"/> that executes the major increase function.
        /// </summary>
        private readonly RoutedUICommand _majorIncreaseValueCommand =
            new("MajorIncreaseValue", "MajorIncreaseValue", typeof(IntegerSpinner));

        /// <summary>
        /// <see cref="RoutedUICommand"/> that executes the major decrease function.
        /// </summary>
        private readonly RoutedUICommand _majorDecreaseValueCommand =
            new("MajorDecreaseValue", "MajorDecreaseValue", typeof(IntegerSpinner));

        //private readonly RoutedUICommand _updateValueStringCommand =
        //            new("UpdateValueString", "UpdateValueString", typeof(IntegerSpinner));

        /// <summary>
        /// <see cref="RoutedUICommand"/> that executes the cancel text changes function.
        /// </summary>
        private readonly RoutedUICommand _cancelChangesCommand =
            new("CancelChanges", "CancelChanges", typeof(IntegerSpinner));

        #endregion

        #region Private Control Fields

        /// <summary>
        /// Gets the reference to the text box of the control.
        /// </summary>
        private TextBox _textBox;

        /// <summary>
        /// Gets the reference to the increase repeat button.
        /// </summary>
        private RepeatButton _increaseButton;

        /// <summary>
        /// Gets the reference to the decrease repeat button.
        /// </summary>
        private RepeatButton _decreaseButton;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerSpinner"/> class. control.
        /// </summary>
        public IntegerSpinner()
        {
            _culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            Loaded += OnLoaded;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerSpinner"/> class control.
        /// </summary>
        static IntegerSpinner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(IntegerSpinner),
                new FrameworkPropertyMetadata(
                    typeof(IntegerSpinner)));
        }

        #endregion

        #region Events

        /// <summary>
        /// Event handler for the control loaded.
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="args">Event argument</param>
        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            InvalidateProperty(ValueProperty);
        }

        #endregion

        #region Routed Events

        /// <summary>
        /// Value changed event handler.
        /// </summary>
        public event RoutedEventHandler ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }

        /// <summary>
        /// Register the value changed routed event handler.
        /// </summary>
        private static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
            name: nameof(ValueChanged),
            routingStrategy: RoutingStrategy.Bubble,
            handlerType: typeof(RoutedEventHandler),
            ownerType: typeof(IntegerSpinner));

        /// <summary>
        /// Method that raises the value changed event.
        /// </summary>
        void RaiseValueChangedEvent()
        {
            // Create a Routed Event Args instance.
            RoutedEventArgs args = new(routedEvent: ValueChangedEvent);

            // Raise the event, which will bubble up through the element tree.
            RaiseEvent(args);
        }

        #endregion

        #region Value Dependency Property

        /// <summary>
        /// Gets or sets the value dependency backing property.
        /// </summary>
        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        //public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        //    nameof(Value),
        //    typeof(int),
        //    typeof(IntegerSpinner),
        //    new PropertyMetadata(default(int), OnValueChanged, CoerceValue));

        /// <summary>
        /// Register the Value dependency property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(int),
            typeof(IntegerSpinner),
            new FrameworkPropertyMetadata(default(int),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged,
                CoerceValue,
                true,
                UpdateSourceTrigger.PropertyChanged));

        /// <summary>
        /// Value Changed dependency meta data event handler.
        /// </summary>
        /// <param name="element">The dependency object</param>
        /// <param name="args">The dependency property changed event argument</param>
        private static void OnValueChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var control = (IntegerSpinner)element;

            // Get the new value
            var value = (int)args.NewValue;

            if (control._increaseButton != null && control._decreaseButton != null)
            {
                // Disable the increase button when the 
                control._increaseButton.IsEnabled = value < control.MaxValue;
                control._decreaseButton.IsEnabled = value > control.MinValue;
            }
        }

        private static object CoerceValue(DependencyObject element, object baseValue)
        {
            var control = (IntegerSpinner)element;
            var value = (int)baseValue;

            // Coerce value to the selected bounds
            control.CoerceValueToBounds(ref value);

            if (control._textBox != null)
            {

                control._textBox.Text = control.HasThousandSeparator // If control should have a thousand separator,
                    ? value.ToString("N0", control._culture) // Change to a comma separated number format.
                    : value.ToString("####", control._culture); // Use fixed point number format
                control._textBox.CaretIndex = control._textBox.Text.Length;
                // control.TextBox.Text = value.ToString("G", control._culture);
            }

            return value;
        }

        /// <summary>
        /// Applies boundaries to the <see cref="Value"/> every time it changes.
        /// </summary>
        /// <param name="value">The value to  be coerced</param>
        private void CoerceValueToBounds(ref int value)
        {
            if (value < MinValue)
            {
                value = MinValue;
            }
            else if (value > MaxValue)
            {
                value = MaxValue;
            }
        }

        #endregion

        #region Max Value Dependency Property

        /// <summary>
        /// Gets or sets the max value dependency backing property.
        /// </summary>
        public int MaxValue
        {
            get => (int)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        /// <summary>
        /// Register the Maximum Value dependency property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
            nameof(MaxValue),
            typeof(int),
            typeof(IntegerSpinner),
            new PropertyMetadata(int.MaxValue, OnMaxValueChanged, CoerceMaxValue));

        private static void OnMaxValueChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var control = (IntegerSpinner)element;
            var maxvalue = (int)args.NewValue;

            // If max value steps over min value, shift it
            if (maxvalue < control.MinValue)
            {
                control.MinValue = maxvalue;
            }

            if (maxvalue <= control.Value)
            {
                control.Value = maxvalue;
            }
        }

        private static object CoerceMaxValue(DependencyObject element, object baseValue)
        {
            return (int)baseValue;
        }

        #endregion

        #region Min Value Dependency Property

        /// <summary>
        /// Gets or sets the min value dependency backing property.
        /// </summary>
        public int MinValue
        {
            get => (int)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        /// <summary>
        /// Registers the Min Value Dependency Property.
        /// </summary>
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
            nameof(MinValue),
            typeof(int),
            typeof(IntegerSpinner),
            new PropertyMetadata(int.MinValue, OnMinValueChanged, CoerceMinValue));

        private static void OnMinValueChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var control = (IntegerSpinner)element;
            var minValue = (int)args.NewValue;

            //If min value steps over max value, shift it
            if (minValue > control.MaxValue)
            {
                control.MaxValue = minValue;
            }

            if (minValue >= control.Value)
            {
                control.Value = minValue;
            }
        }

        private static object CoerceMinValue(DependencyObject element, object baseValue)
        {
            return (int)baseValue;
        }

        #endregion

        #region Has Thousand Separator Dependency Property

        /// <summary>
        /// Gets or sets dependency backing property indicating whether the control should have a thousand separator..
        /// </summary>
        public bool HasThousandSeparator
        {
            get => (bool)GetValue(HasThousandSeparatorProperty);
            set => SetValue(HasThousandSeparatorProperty, value);
        }

        /// <summary>
        /// Register the Has Thousand Separator dependency property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty HasThousandSeparatorProperty = DependencyProperty.Register(
            nameof(HasThousandSeparator),
            typeof(bool),
            typeof(IntegerSpinner),
            new PropertyMetadata(default(bool), OnHasThousandSeparatorChanged));

        private static void OnHasThousandSeparatorChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var control = (IntegerSpinner)element;
            control.InvalidateProperty(ValueProperty);
        }

        #endregion

        #region Can Auto Select Dependency Property

        /// <summary>
        /// Gets or sets a value indicating whether the value in the control can be auto selected.
        /// </summary>
        public bool CanAutoSelect
        {
            get => (bool)GetValue(CanAutoSelectProperty);
            set => SetValue(CanAutoSelectProperty, value);
        }

        /// <summary>
        /// Register the Can Auto Select dependency property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty CanAutoSelectProperty =
            DependencyProperty.Register(
                nameof(CanAutoSelect),
                typeof(bool),
                typeof(IntegerSpinner),
                new PropertyMetadata(false));

        #endregion

        #region Minor Step Dependency Property

        /// <summary>
        /// Gets or sets minor step dependency backing property that determines the minimum value to add or subtract
        /// from the <see cref="Value"/>.
        /// </summary>
        public int MinorStep
        {
            get => (int)GetValue(MinorStepProperty);
            set => SetValue(MinorStepProperty, value);
        }

        /// <summary>
        /// Register the MinorStep dependency property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty MinorStepProperty =
            DependencyProperty.Register(
                nameof(MinorStep),
                typeof(int),
                typeof(IntegerSpinner),
                new PropertyMetadata(1, OnMinorStepChanged, CoerceMinorDelta));

        private static void OnMinorStepChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var minorStep = (int)args.NewValue;
            var control = (IntegerSpinner)element;

            if (minorStep > control.MajorStep)
            {
                control.MajorStep = minorStep;
            }
        }

        private static object CoerceMinorDelta(DependencyObject element, object baseValue)
        {
            return (int)baseValue;
        }

        #endregion

        #region Major Step Dependency Property

        /// <summary>
        /// Gets or sets major step dependency backing property that determines the maximum value to add or subtract
        /// from the <see cref="Value"/>.
        /// </summary>
        public int MajorStep
        {
            get => (int)GetValue(MajorStepProperty);
            set => SetValue(MajorStepProperty, value);
        }

        /// <summary>
        /// Register the MajorStep dependency property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty MajorStepProperty =
            DependencyProperty.Register(
                nameof(MajorStep),
                typeof(int),
                typeof(IntegerSpinner),
                new PropertyMetadata(10, OnMajorStepChanged, CoerceMajorStep));

        private static void OnMajorStepChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var majorStep = (int)args.NewValue;
            var control = (IntegerSpinner)element;

            if (majorStep < control.MinorStep)
            {
                control.MinorStep = majorStep;
            }
        }

        private static object CoerceMajorStep(DependencyObject element, object baseValue)
        {
            return (int)baseValue;
        }

        #endregion

        #region Can Undo Dependency Property

        /// <summary>
        /// Gets or sets a value indicating whether the control can undo it's value.
        /// </summary>
        public bool CanUndo
        {
            get => (bool)GetValue(CanUndoProperty);
            set => SetValue(CanUndoProperty, value);
        }

        /// <summary>
        /// Register the Can Undo dependency property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty CanUndoProperty =
            DependencyProperty.Register(
                nameof(CanUndo),
                typeof(bool),
                typeof(IntegerSpinner),
                new PropertyMetadata(false));

        #endregion

        #region Corner Radius

        /// <summary>
        /// Gets or sets the degree at which the corners of the Numeric Spinner is rounded.
        /// </summary>
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// Register the Corner Radius dependency property.
        /// </summary>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(IntegerSpinner),
                new FrameworkPropertyMetadata(new CornerRadius(), 
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region Overrides of Control

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AttachToVisualTree();
            AttachCommands();
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Attach control parts to the visual tree.
        /// </summary>
        private void AttachToVisualTree()
        {
            AttachIncreaseButton();
            AttachDecreaseButton();
            AttachTextBox();
        }

        /// <summary>
        /// Attach command and input bindings to command methods. 
        /// </summary>
        private void AttachCommands()
        {
            CommandBindings.Add(new CommandBinding(_minorIncreaseValueCommand, (_, _) => IncreaseValue(true)));
            _textBox.InputBindings.Add(new KeyBinding(_minorIncreaseValueCommand, new KeyGesture(Key.Up)));

            CommandBindings.Add(new CommandBinding(_minorDecreaseValueCommand, (_, _) => DecreaseValue(true)));
            _textBox.InputBindings.Add(new KeyBinding(_minorDecreaseValueCommand, new KeyGesture(Key.Down)));

            CommandBindings.Add(new CommandBinding(_majorIncreaseValueCommand, (_, _) => IncreaseValue(false)));
            _textBox.InputBindings.Add(new KeyBinding(_majorIncreaseValueCommand, new KeyGesture(Key.PageUp)));

            CommandBindings.Add(new CommandBinding(_majorDecreaseValueCommand, (_, _) => DecreaseValue(false)));
            _textBox.InputBindings.Add(new KeyBinding(_majorDecreaseValueCommand, new KeyGesture(Key.PageDown)));

            //CommandBindings.Add(new CommandBinding(_updateValueStringCommand, (_, _) => UpdateValue()));
            //TextBox.InputBindings.Add(new KeyBinding(_updateValueStringCommand, new KeyGesture(Key.Enter)));

            CommandBindings.Add(new CommandBinding(_cancelChangesCommand, (_, _) => CancelChanges()));
            _textBox.InputBindings.Add(new KeyBinding(_cancelChangesCommand, new KeyGesture(Key.Escape)));
            //CommandManager.RegisterClassInputBinding(typeof(TextBox),
            //                new KeyBinding(_minorIncreaseValueCommand, new KeyGesture(Key.Up)));
            //CommandManager.RegisterClassInputBinding(typeof(TextBox),
            //    new KeyBinding(_minorDecreaseValueCommand, new KeyGesture(Key.Down)));
        }

        /// <summary>
        /// Passes and deletes focus on the <see cref="IntegerSpinner"/> control.
        /// </summary>
        private void RemoveFocus()
        {
            // Passes focus here and just deletes it.
            Focusable = true;
            Focus();
            Focusable = false;
        }

        #endregion

        #region Text Box

        /// <summary>
        /// Method that configures the text box part before attaching it.
        /// </summary>
        private void AttachTextBox()
        {
            // Get the text box part and cast it as text box.
            _textBox = (TextBox)GetTemplateChild(TextBoxName)!;
            _textBox.TextChanged += TextBox_TextChanged;
            _textBox.PreviewMouseLeftButtonUp += TextBox_PreviewMouseLeftButtonUp;

            // Set undo limit
            _textBox.UndoLimit = 1;
            _textBox.IsUndoEnabled = CanUndo;
        }

        /// <summary>
        /// Preview Mouse Left Button Up event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void TextBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
        {
            if (CanAutoSelect)
            {
                _textBox.SelectAll();
            }
        }

        /// <summary>
        /// Event handler when the text in the text box is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            Value = ParseStringToInteger(_textBox.Text);
            RaiseValueChangedEvent();
        }

        //private void UpdateValue()
        //{
        //    Value = ParseStringToInteger(TextBox.Text);
        //}

        /// <summary>
        /// Method that undo text box value change.
        /// </summary>
        private void CancelChanges()
        {
            _textBox.Undo();
        }

        #endregion

        #region Increase Button

        /// <summary>
        /// Method that configures the increase button before attaching it.
        /// </summary>
        private void AttachIncreaseButton()
        {
            _increaseButton = (RepeatButton)GetTemplateChild(IncreaseButtonName)!;
            _increaseButton.Focusable = false;
            _increaseButton.Command = _minorIncreaseValueCommand;

            // Set value to the max value when increase button is right clicked.
            _increaseButton.PreviewMouseRightButtonDown += (_, _) => { Value = MaxValue; };

            // Remove focus when increase button is left clicked.
            _increaseButton.PreviewMouseLeftButtonDown += (_, _) => RemoveFocus();
        }

        /// <summary>
        /// Method that increases the <see cref="Value"/> by the minor or major step
        /// </summary>
        /// <param name="minor">A value indicating whether to increase value by minor or major step</param>
        private void IncreaseValue(bool minor)
        {
            // Get the value that's currently in the text box
            var value = ParseStringToInteger(_textBox.Text);

            // Coerce value to min/max
            CoerceValueToBounds(ref value);

            // Only change the value if it has any meaning.
            if (value < MaxValue)
            {
                // Increase the current value
                value = minor ? value + MinorStep : value + MajorStep;
            }

            // Set the current value to the text box value.
            Value = value;
        }

        #endregion

        #region Decrease Button

        /// <summary>
        /// Method that configures the decrease button before attaching it.
        /// </summary>
        private void AttachDecreaseButton()
        {
            _decreaseButton = (RepeatButton)GetTemplateChild(DecreaseButtonName)!;
            _decreaseButton.Focusable = false;
            _decreaseButton.Command = _minorDecreaseValueCommand;
            // Set value to the max value when increase button is right clicked.
            _decreaseButton.PreviewMouseRightButtonDown += (_, _) => { Value = MinValue; };
            _decreaseButton.PreviewMouseLeftButtonDown += (_, _) => RemoveFocus();
        }

        /// <summary>
        /// Method that decreases the <see cref="Value"/> by the minor or major step
        /// </summary>
        /// <param name="minor">A value indicating whether to decrease value by minor step or major step</param>
        private void DecreaseValue(bool minor)
        {
            // Get the value that's currently in the text box
            var value = ParseStringToInteger(_textBox.Text);

            // Coerce the value to min/max
            CoerceValueToBounds(ref value);

            if (value > MinValue)
            {
                // Decrease the current value
                value = minor ? value - MinorStep : value - MajorStep;
            }

            // Set the current value to the text box value.
            Value = value;
        }

        #endregion

        #region Parse String To Integer

        /// <summary>
        /// Attempts to parse a string to an integer.
        /// </summary>
        /// <param name="source">The string to parse.</param>
        /// <returns></returns>
        private static int ParseStringToInteger(string source)
        {
            IFormatProvider provider = CultureInfo.CurrentCulture;
            _ = int.TryParse(source, NumberStyles.Integer | NumberStyles.AllowThousands, provider, out int value);
            return value;
        }

        #endregion
    }
}
