using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace Serenity.Media
{
    /// <summary>
    ///   checks stream data if valid image file and validate required conditions.
    ///   </summary>
    public class ImageChecker
    {
        /// <summary>The size of stream data being validated</summary>
        private long dataSize;
        /// <summary>The verified image width</summary>
        private int width;
        /// <summary>The verified image height</summary>
        private int height;
        /// <summary>The elapsed time to check whether image is valid or not</summary>
        private double milliseconds;

        /// <summary>Maximum data size limit</summary>
        private int maxDataSize;
        /// <summary>Maximum image width</summary>
        private int maxWidth;
        /// <summary>Maximum image height</summary>
        private int maxHeight;
        /// <summary>Minimum width</summary>
        private int minWidth;
        /// <summary>Minimum height</summary>
        private int minHeight;

        /// <summary>
        ///   Checks if the given image if it is a valid or not. If so, controls its compliance to constraints</summary> 
        /// <param name="inputStream">
        ///   Stream which contains image data</param>
        /// <param name="returnImage">
        ///   Does image required to be returned? If not requested, it will be disposed at the end of processing</param>
        /// <param name="image">
        ///   When method returns contains the image object. If returnImage false it will contain null</param>
        /// <returns>
        ///   Image validation result. One of <see cref="ImageCheckResult"/> values. If the result is one of
        ///   GIFImage, JPEGImage, PNGImage, the checking is successful. Rest of results are invalid.</returns>
        public ImageCheckResult CheckStream(Stream inputStream, bool returnImage, out Image image)
        {
            // store the start time of validation
            DateTime startTime = DateTime.Now;

            // initialize result variables
            dataSize = 0;
            width = 0;
            height = 0;
            milliseconds = -1;
            image = null;
            
            try
            {
                // set stream position to start
                inputStream.Seek(0, SeekOrigin.Begin);

                // set file data size to input stream length
                dataSize = inputStream.Length;
            }
            catch
            {
                return ImageCheckResult.StreamReadError;
            }

            // if data size equals 0 return StreamReadError
            if (dataSize == 0)
                return ImageCheckResult.StreamReadError;

            // check file size
            if (maxDataSize != 0 && dataSize > maxDataSize)
                return ImageCheckResult.DataSizeTooHigh;

            // try to upload image
            try
            {
                // load image validating it
                image = Image.FromStream(inputStream, true, true);
            }
            catch
            {
                // couldn't load image
                return ImageCheckResult.InvalidImage;
            }

            bool isImageOK = false;

            try
            {
                ImageCheckResult checkResult;

                // check image format, if not JPEG, PNG or GIF return UnsupportedFormat error
                if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
                    checkResult = ImageCheckResult.JPEGImage;
                else if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                    checkResult = ImageCheckResult.GIFImage;
                else if (image.RawFormat.Equals(System.Drawing.Imaging.ImageFormat.Png))
                    checkResult = ImageCheckResult.PNGImage;
                else
                    return ImageCheckResult.UnsupportedFormat;

                // read image size
                width = image.Width;
                height = image.Height;

                var constraintResult = CheckSizeConstraints(width, height);

                if (constraintResult != ImageCheckResult.JPEGImage)
                    return constraintResult;

                TimeSpan span = DateTime.Now.Subtract(startTime);
                milliseconds = span.TotalMilliseconds;
                
                isImageOK = true;

                return checkResult;
            }
            finally
            {
                if (!(isImageOK && returnImage))
                {
                    image.Dispose();
                    image = null;
                }
            }
        }

        /// <summary>
        ///   Checks an image width and height against size constraints</summary>
        /// <param name="width">
        ///   Image width.</param>
        /// <param name="height">
        ///   Image height.</param>
        /// <returns>
        ///   One of ImageCheckResult values. ImageCheckResult.JPEGImage if image size is validated.</returns>
        public ImageCheckResult CheckSizeConstraints(int width, int height)
        {
            // raise an error if width or height is zero
            if (width == 0 || height == 0)
                return ImageCheckResult.ImageIsEmpty;

            // check minimum height and width
            if (width < MinWidth)
            {
                if (MinWidth == MaxWidth)
                {
                    if (MinHeight == MaxHeight)
                        return ImageCheckResult.SizeMismatch;
                    else
                        return ImageCheckResult.WidthMismatch;

                }
                else
                    return ImageCheckResult.WidthTooLow;
            }

            if (height < MinHeight)
            {
                if (MinHeight == MaxHeight)
                    return ImageCheckResult.HeightMismatch;
                else
                    return ImageCheckResult.HeightTooLow;
            }

            // check maximum height and width
            if (MaxWidth != 0 && width > MaxWidth)
                return ImageCheckResult.WidthTooHigh;

            if (MaxHeight != 0 && height > MaxHeight)
                return ImageCheckResult.HeightTooHigh;

            return ImageCheckResult.JPEGImage;
        }

        /// <summary>
        ///   Use this overload to check an image from a stream.</summary>
        /// <param name="inputStream">
        ///   Stream with image data</param>
        /// <returns>
        ///   Image checking result</returns>
        /// <seealso cref="CheckStream(Stream, bool, out Image)"/>
        public ImageCheckResult CheckStream(Stream inputStream)
        {
            Image image;
            return CheckStream(inputStream, false, out image);
        }

        /// <summary>
        ///   Gets data size of the validated image</summary>
        public long DataSize
        {
            get { return dataSize; }
        }

        /// <summary>
        ///   Gets width of the validated image</summary>
        public int Width
        {
            get { return width; }
        }

        /// <summary>
        ///   Gets height of the validate image</summary>
        public int Height
        {
            get { return height; }
        }

        /// <summary>
        ///   Gets the time passed during validating the image</summary>
        public double Milliseconds
        {
            get { return milliseconds; }
        }

        /// <summary>
        ///   Gets/sets maximum file size allowed</summary>
        public int MaxDataSize
        {
            get { return maxDataSize; }
            set { maxDataSize = value; }
        }

        /// <summary>
        ///   Gets/sets maximum width allowed. 0 means any width.</summary>
        public int MaxWidth
        {
            get { return maxWidth; }
            set { maxWidth = value; }
        }

        /// <summary>
        ///   Gets/sets maximum height allowed. 0 means any height.</summary>
        public int MaxHeight
        {
            get { return maxHeight; }
            set { maxHeight = value; }
        }

        /// <summary>
        ///   Gets/sets minimum width allowed. 0 means any width.</summary>
        public int MinWidth
        {
            get { return minWidth; }
            set { minWidth = value; }
        }

        /// <summary>
        ///   Gets/sets minimum height allowed. 0 means any height.</summary>
        public int MinHeight
        {
            get { return minHeight; }
            set { minHeight = value; }
        }

        /// <summary>
        ///   Formats an error message for a ImageCheckResult code returned from a CheckStream operation.
        ///   This should be called right after CheckStream, cause image checkers current constraint
        ///   and results will be used to format the message.</summary>
        /// <param name="error">
        ///   Checking result.</param>
        /// <returns>
        ///   An error message formatted according to constraints and validation result.</returns>
        public string FormatErrorMessage(ImageCheckResult error)
        {
            return
                String.Format(
                    ImageChecker.CheckErrorMessages[(int)error],
                    DataSize, // 0
                    Width, // 1
                    Height, // 2
                    MaxDataSize, // 3
                    MinWidth, // 4
                    MinHeight, // 5
                    MaxWidth, // 6
                    MaxHeight); // 7
        }

        /// <summary>
        ///   Set of error texts used by FormatErrorMessage.</summary>
        private static readonly LocalText[] CheckErrorMessages = new LocalText[] 
        {   
            // GIF images not supported! (if an error)
            "cms.image_check_result.gif_image",
            // JPEG images not supported! (if an error)
            "cms.image_check_result.jpeg_image",
            // PNG images not supported! (if an error)
            "cms.image_check_result.png_image",
            // Flash (.swf) movies is not supported!
            "cms.image_check_result.flash_movie",
            // Unsupported image format (BMP etc)
            "cms.image_check_result.unsupported_format",
            // Stream read error
            "cms.image_check_result.stream_read_error",
            // Image size too hight
            "cms.image_check_result.data_size_too_high",
            // Image is invalid
            "cms.image_check_result.invalid_image",
            // Image is empty
            "cms.image_check_result.image_is_empty",
            // Image size mismatch
            "cms.image_check_result.size_mismatch",
            // Image width mismatch
            "cms.image_check_result.width_mismatch",
            // Image width too high
            "cms.image_check_result.width_too_high",
            // Image width too low
            "cms.image_check_result.width_too_low",
            // Image height mismatch
            "cms.image_check_result.height_mismatch",
            // Image height too high
            "cms.image_check_result.height_too_high",
            // Image height too low
            "cms.image_check_result.height_too_low"
        };
    }
}