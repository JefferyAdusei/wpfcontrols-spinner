using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Spyder.Controls.Spinner
{
    /// <summary>
    /// Custom Control for handling numeric values.
    /// </summary>
    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_IncreaseButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "PART_DecreaseButton", Type = typeof(RepeatButton))]
    public class NumericSpinner : Control
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

        /// <summary>
        /// Minimum decimal place
        /// </summary>
        private const int MINDECIMALPLACE = 0;

        /// <summary>
        /// Maximum decimal place
        /// </summary>
        private const int MAXDECIMALPLACE = 28;

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
            new("MinorIncreaseValue", "MinorIncreaseValue", typeof(NumericSpinner));

        /// <summary>
        /// <see cref="RoutedUICommand"/> that executes the minor decrease function.
        /// </summary>
        private readonly RoutedUICommand _minorDecreaseValueCommand =
            new("MinorDecreaseValue", "MinorDecreaseValue", typeof(NumericSpinner));

        /// <summary>
        /// <see cref="RoutedUICommand"/> that executes the major increase function.
        /// </summary>
        private readonly RoutedUICommand _majorIncreaseValueCommand =
            new("MajorIncreaseValue", "MajorIncreaseValue", typeof(NumericSpinner));

        /// <summary>
        /// <see cref="RoutedUICommand"/> that executes the major decrease function.
        /// </summary>
        private readonly RoutedUICommand _majorDecreaseValueCommand =
            new("MajorDecreaseValue", "MajorDecreaseValue", typeof(NumericSpinner));

        /// <summary>
        /// <see cref="RoutedUICommand"/> that executes the update value function.
        /// </summary>
        private readonly RoutedUICommand _updateValueStringCommand =
                    new("UpdateValueString", "UpdateValueString", typeof(NumericSpinner));

        /// <summary>
        /// <see cref="RoutedUICommand"/> that executes the cancel text changes function.
        /// </summary>
        private readonly RoutedUICommand _cancelChangesCommand =
            new("CancelChanges", "CancelChanges", typeof(NumericSpinner));

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
        /// Initializes a new instance of the <see cref="NumericSpinner"/> class. control.
        /// </summary>
        public NumericSpinner()
        {
            _culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            _culture.NumberFormat.NumberDecimalDigits = DecimalPlaces;

            Loaded += OnLoaded;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericSpinner"/> class control.
        /// </summary>
        static NumericSpinner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericSpinner),
                new FrameworkPropertyMetadata(
                    typeof(NumericSpinner)));
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
            ownerType: typeof(NumericSpinner));

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
        public decimal Value
        {
            get => (decimal)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        /// <summary>
        /// Register the Value dependency property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            nameof(Value),
            typeof(decimal),
            typeof(NumericSpinner),
            new FrameworkPropertyMetadata(default(decimal),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                OnValueChanged,
                CoerceValue,
                true,
                UpdateSourceTrigger.LostFocus));

        /// <summary>
        /// Value Changed dependency meta data event handler.
        /// </summary>
        /// <param name="element">The dependency object</param>
        /// <param name="args">The dependency property changed event argument</param>
        private static void OnValueChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var control = (NumericSpinner)element;

            // Get the new value
            var value = (decimal)args.NewValue;

            if (control._increaseButton != null && control._decreaseButton != null)
            {
                // Disable the increase button when the 
                control._increaseButton.IsEnabled = value < control.MaxValue;
                control._decreaseButton.IsEnabled = value > control.MinValue;
            }
        }

        private static object CoerceValue(DependencyObject element, object baseValue)
        {
            var control = (NumericSpinner)element;
            var value = (decimal)baseValue;

            // Coerce value to the selected bounds
            control.CoerceValueToBounds(ref value);

            // Get the text representation of Value
            var valueString = value.ToString(control._culture);

            // Count all decimal places
            var decimals = control.GetDecimalPlacesCount(valueString);

            if (decimals > control.DecimalPlaces)
            {
                // Remove all overflowing decimal places
                value = control.TruncateValue(valueString, decimals);
            }

            if (control._textBox != null)
            {
                control._textBox.Text = control.HasThousandSeparator // If control should have a thousand separator,
                    ? value.ToString("N", control._culture) // Change to a comma separated number format.
                    : value.ToString("F", control._culture); // Use fixed point number format

                // control.TextBox.Text = value.ToString("G", control._culture);
            }

            return value;
        }

        /// <summary>
        /// Applies boundaries to the <see cref="Value"/> every time it changes.
        /// </summary>
        /// <param name="value">The value to  be coerced</param>
        private void CoerceValueToBounds(ref decimal value)
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
        public decimal MaxValue
        {
            get => (decimal)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        /// <summary>
        /// Register the Maximum Value dependency property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(
            nameof(MaxValue),
            typeof(decimal),
            typeof(NumericSpinner),
            new PropertyMetadata(decimal.MaxValue, OnMaxValueChanged, CoerceMaxValue));

        private static void OnMaxValueChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var control = (NumericSpinner)element;
            var maxvalue = (decimal)args.NewValue;

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
            return (decimal)baseValue;

        }

        #endregion

        #region Min Value Dependency Property

        /// <summary>
        /// Gets or sets the min value dependency backing property.
        /// </summary>
        public decimal MinValue
        {
            get => (decimal)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        /// <summary>
        /// Registers the Min Value Dependency Property.
        /// </summary>
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(
            nameof(MinValue),
            typeof(decimal),
            typeof(NumericSpinner),
            new PropertyMetadata(decimal.MinValue, OnMinValueChanged, CoerceMinValue));

        private static void OnMinValueChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var control = (NumericSpinner)element;
            var minValue = (decimal)args.NewValue;

            // If min value steps over max value, shift it
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
            return (decimal)baseValue;

        }

        #endregion

        #region Decimal Places Dependency Property

        /// <summary>
        /// Gets or sets the number of decimal places dependency backing property.
        /// </summary>
        public int DecimalPlaces
        {
            get => (int)GetValue(DecimalPlacesProperty);
            set => SetValue(DecimalPlacesProperty, value);
        }

        /// <summary>
        /// Register the Decimal Places dependency property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register(
            nameof(DecimalPlaces),
            typeof(int),
            typeof(NumericSpinner),
            new PropertyMetadata(default(int), OnDecimalPlacesChanged, CoerceDecimalPlaces));

        private static void OnDecimalPlacesChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var control = (NumericSpinner)element;
            var decimalPlaces = (int)args.NewValue;

            control._culture.NumberFormat.NumberDecimalDigits = decimalPlaces;

            // Tell Value to invalidate itself by running the CoerceValue() method again
            control.InvalidateProperty(ValueProperty);
        }

        private static object CoerceDecimalPlaces(DependencyObject element, object baseValue)
        {
            var decimalPlaces = (int)baseValue;
            var control = (NumericSpinner)element;

            // Decimal places should not be a negative number
            if (decimalPlaces < control.MinDecimalPlaces)
            {
                decimalPlaces = control.MinDecimalPlaces;
            }
            else if (decimalPlaces > control.MaxDecimalPlaces)
            {
                decimalPlaces = control.MaxDecimalPlaces;
            }

            return decimalPlaces;
        }

        #endregion

        #region Max Decimal Places Dependency Property

        /// <summary>
        /// Gets or sets the maximum number of decimal places dependency backing property.
        /// </summary>
        public int MaxDecimalPlaces
        {
            get => (int)GetValue(MaxDecimalPlacesProperty);
            set => SetValue(MaxDecimalPlacesProperty, value);
        }

        /// <summary>
        /// Register the Maximum Decimal Place dependency property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty MaxDecimalPlacesProperty = DependencyProperty.Register(
            nameof(MaxDecimalPlaces),
            typeof(int),
            typeof(NumericSpinner),
            new PropertyMetadata(MAXDECIMALPLACE, OnMaxDecimalPlacesChanged, CoerceMaxDecimalPlaces));

        private static void OnMaxDecimalPlacesChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var control = (NumericSpinner)element;
            control.InvalidateProperty(DecimalPlacesProperty);
        }

        private static object CoerceMaxDecimalPlaces(DependencyObject element, object baseValue)
        {
            var maxDecimalPlaces = (int)baseValue;
            var control = (NumericSpinner)element;

            if (maxDecimalPlaces > MAXDECIMALPLACE)
            {
                maxDecimalPlaces = MAXDECIMALPLACE;
            }
            else if (maxDecimalPlaces < MINDECIMALPLACE)
            {
                maxDecimalPlaces = MINDECIMALPLACE;
            }
            else if (maxDecimalPlaces < control.MinDecimalPlaces)
            {
                control.MinDecimalPlaces = maxDecimalPlaces;
            }

            return maxDecimalPlaces;
        }

        #endregion

        #region Min Decimal Places Dependency Property

        /// <summary>
        /// Gets or sets the minimum decimal places dependency backing property.
        /// </summary>
        public int MinDecimalPlaces
        {
            get => (int)GetValue(MinDecimalPlacesProperty);
            set => SetValue(MinDecimalPlacesProperty, value);
        }

        /// <summary>
        /// Register the Minimum Decimal Place dependency property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty MinDecimalPlacesProperty = DependencyProperty.Register(
            nameof(MinDecimalPlaces),
            typeof(int),
            typeof(NumericSpinner),
            new PropertyMetadata(MINDECIMALPLACE, OnMinDecimalPlacesChanged, CoerceMinDecimalPlaces));

        private static void OnMinDecimalPlacesChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var control = (NumericSpinner)element;
            control.InvalidateProperty(DecimalPlacesProperty);
        }

        private static object CoerceMinDecimalPlaces(DependencyObject element, object baseValue)
        {
            var minDecimalPlaces = (int)baseValue;
            var control = (NumericSpinner)element;

            if (minDecimalPlaces < MINDECIMALPLACE)
            {
                minDecimalPlaces = MINDECIMALPLACE;
            }
            else if (minDecimalPlaces > MAXDECIMALPLACE)
            {
                minDecimalPlaces = MAXDECIMALPLACE;
            }
            else if (minDecimalPlaces < control.MinDecimalPlaces)
            {
                control.MinDecimalPlaces = minDecimalPlaces;
            }

            return minDecimalPlaces;
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
            typeof(NumericSpinner),
            new PropertyMetadata(default(bool), OnHasThousandSeparatorChanged));

        private static void OnHasThousandSeparatorChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var control = (NumericSpinner)element;
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
                typeof(NumericSpinner),
                new PropertyMetadata(false));

        #endregion

        #region Minor Step Dependency Property

        /// <summary>
        /// Gets or sets minor step dependency backing property that determines the minimum value to add or subtract
        /// from the <see cref="Value"/>.
        /// </summary>
        public decimal MinorStep
        {
            get => (decimal)GetValue(MinorStepProperty);
            set => SetValue(MinorStepProperty, value);
        }

        /// <summary>
        /// Register the MinorStep dependency property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty MinorStepProperty =
            DependencyProperty.Register(
                nameof(MinorStep),
                typeof(decimal),
                typeof(NumericSpinner),
                new PropertyMetadata(decimal.One, OnMinorStepChanged, CoerceMinorDelta));

        private static void OnMinorStepChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var minorStep = (decimal)args.NewValue;
            var control = (NumericSpinner)element;

            if (minorStep > control.MajorStep)
            {
                control.MajorStep = minorStep;
            }
        }

        private static object CoerceMinorDelta(DependencyObject element, object baseValue)
        {
            return (decimal)baseValue;
        }

        #endregion

        #region Major Step Dependency Property

        /// <summary>
        /// Gets or sets major step dependency backing property that determines the maximum value to add or subtract
        /// from the <see cref="Value"/>.
        /// </summary>
        public decimal MajorStep
        {
            get => (decimal)GetValue(MajorStepProperty);
            set => SetValue(MajorStepProperty, value);
        }

        /// <summary>
        /// Register the MajorStep dependency property. This enables animation, styling, binding, etc...
        /// </summary>
        public static readonly DependencyProperty MajorStepProperty =
            DependencyProperty.Register(
                nameof(MajorStep),
                typeof(decimal),
                typeof(NumericSpinner),
                new PropertyMetadata(10m, OnMajorStepChanged, CoerceMajorStep));

        private static void OnMajorStepChanged(DependencyObject element, DependencyPropertyChangedEventArgs args)
        {
            var majorStep = (decimal)args.NewValue;
            var control = (NumericSpinner)element;

            if (majorStep < control.MinorStep)
            {
                control.MinorStep = majorStep;
            }
        }

        private static object CoerceMajorStep(DependencyObject element, object baseValue)
        {
            return (decimal)baseValue;
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
                typeof(NumericSpinner),
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
            typeof(NumericSpinner),
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

            CommandBindings.Add(new CommandBinding(_updateValueStringCommand, (_, _) => UpdateValue()));
            _textBox.InputBindings.Add(new KeyBinding(_updateValueStringCommand, new KeyGesture(Key.Enter)));

            CommandBindings.Add(new CommandBinding(_cancelChangesCommand, (_, _) => CancelChanges()));
            _textBox.InputBindings.Add(new KeyBinding(_cancelChangesCommand, new KeyGesture(Key.Escape)));
            //CommandManager.RegisterClassInputBinding(typeof(TextBox),
            //                new KeyBinding(_minorIncreaseValueCommand, new KeyGesture(Key.Up)));
            //CommandManager.RegisterClassInputBinding(typeof(TextBox),
            //    new KeyBinding(_minorDecreaseValueCommand, new KeyGesture(Key.Down)));
        }

        /// <summary>
        /// Passes and deletes focus on the <see cref="NumericSpinner"/> control.
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
            _textBox.LostFocus += TextBox_LostFocus;
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
        /// Event handler when the text box loses focus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Value = ParseStringToDecimal(_textBox.Text);
            RaiseValueChangedEvent();
        }

        /// <summary>
        /// Parses the text box value, and updates the <see cref="Value"/>
        /// </summary>
        private void UpdateValue()
        {
            Value = ParseStringToDecimal(_textBox.Text);
        }

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
            var value = ParseStringToDecimal(_textBox.Text);

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
            var value = ParseStringToDecimal(_textBox.Text);

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

        #region Parse String To Decimal

        /// <summary>
        /// Parses a string input to decimal
        /// </summary>
        /// <param name="source">The string value to parse to decimal</param>
        /// <returns></returns>
        private static decimal ParseStringToDecimal(string source)
        {
            if (!decimal.TryParse(source, out decimal value))
            {
                return 0;
            }

            return value;
        }

        #endregion

        #region Get Decimal Places Count

        /// <summary>
        /// Finds the first decimal point and then counts all characters that follow.
        /// </summary>
        /// <param name="source">The source string to compute on.</param>
        /// <returns></returns>
        private int GetDecimalPlacesCount(string source)
        {
            return source
                .SkipWhile(value => value.ToString(_culture) != _culture.NumberFormat.NumberDecimalSeparator)
                .Skip(1)
                .Count();
        }

        #endregion

        #region Truncate Value

        /// <summary>
        /// Deletes the unwanted characters from the end and parses the changed string back into decimal.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        private decimal TruncateValue(string source, int decimalPlaces)
        {
            var endPoint = source.Length - (decimalPlaces - DecimalPlaces);
            var tempValue = source[..endPoint];

            return decimal.Parse(tempValue, _culture);
        }

        #endregion
    }
}
