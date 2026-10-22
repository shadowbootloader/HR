using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Xml.Linq;

namespace HR.Services
{
    public static class ValidationHelper
    {
        public static void SetError(
            TextBox textBox,
            string message)
        {
            var bindingExpression =
                textBox.GetBindingExpression(
                    TextBox.TextProperty);

            if (bindingExpression == null)
                return;

            var error = new ValidationError(
                new ManualValidationRule(),
                bindingExpression,
                message,
                null);

            Validation.MarkInvalid(
                bindingExpression,
                error);
        }


        public static void ClearError(
            TextBox textBox)
        {
            var bindingExpression =
                textBox.GetBindingExpression(
                    TextBox.TextProperty);

            if (bindingExpression == null)
                return;

            Validation.ClearInvalid(
                bindingExpression);
        }

        public static bool CheckEmpty(TextBox textBox)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                

                return false;
            }
            return true;
        }
        private class ManualValidationRule
            : ValidationRule
        {
            public override ValidationResult Validate(
                object value,
                System.Globalization.CultureInfo cultureInfo)
            {
                return ValidationResult.ValidResult;
            }
        }
    }
}
