# WPF Custom Numeric & Integer Spinners
A custom numeric and integer spinner controls

# Numeric Spinner

## Properties
1. **Value** _decimal_ - The bindable decimal value.
2. **MaxValue** _decimal_ - The maximum value boundary the decimal value can reach.
3. **MinValue** _decimal_ - The minimum value boundary the decimal value can reach.
4. **DecimalPlaces** _int_ - The number of decimal places (default is 0)
5. **MaxDecimalPlaces** _int_ - The upper decimal places boundary
6. **MinDecimalPlaces** _int_ - The lower decimal places boundary
7. **HasThousandSeparator** _bool_ - A boolean indicating whether the control should have a thousand separator.
8. **CanAutoSelect** _bool_ - A boolean indicating whether the value in the control can be auto selected when clicked.
9. **MinorStep** _decimal_ - The minimum step to increase or decrease the value by when the increase or decrease commands are requested.
10. **MajorStep** _decimal_ - The major step to increase or decrease the value by when page up or page down keys are pressed.
11. **CanUndo** _bool_ - A boolean indicating whether the control can undo it's value.

# Integer Spinner

## Properties
1. **Value** _int_ - The bindable integer value.
2. **MaxValue** _int_ - The maximum value boundary the integer value can reach.
3. **MinValue** _int_ - The minimum value boundary the integer value can reach.
4. **HasThousandSeparator** _bool_ - A boolean indicating whether the control should have a thousand separator.
5. **CanAutoSelect** _bool_ - A boolean indicating whether the value in the control can be auto selected when clicked.
6. **MinorStep** _int_ - The minimum step to increase or decrease the value by when the increase or decrease commands are requested.
7. **MajorStep** _int_ - The major step to increase or decrease the value by when page up or page down keys are pressed.
8. **CanUndo** _bool_ - A boolean indicating whether the control can undo it's value.

# Usage

Add the namespace `xmlns:spinner="clr-namespace:Spyder.Controls.Spinner;assembly=Spyder.Controls.Spinner"`

Use the control

        <StackPanel HorizontalAlignment="Center" VerticalAlignment="Center">
            <spinner:NumericSpinner
                Background="Black"
                BorderThickness="0"
                CornerRadius="4"
                FontSize="15"
                Foreground="Red" />
            <spinner:IntegerSpinner
                Margin="2"
                Padding="3"
                Background="White"
                BorderThickness="0"
                CornerRadius="4"
                FontSize="12"
                Foreground="Violet" />
        </StackPanel>