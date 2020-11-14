using System;
using Sharprompt.Validations;

namespace PackCreator {
    public static class FileValidators {
        internal static Func<object, ValidationResult> ValidFileName(bool allowEmpty = false) => (obj) => {
            return (!allowEmpty && string.IsNullOrWhiteSpace(obj.ToString())) || FilePathHasInvalidChars(obj.ToString())
                ? new ValidationResult("Name contains invalid characters!")
                : null;
        };

        private static bool FilePathHasInvalidChars(string path) {

            return (!string.IsNullOrEmpty(path) && path.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0);
        }
    }
}