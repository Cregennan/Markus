using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace Markus.Services
{
    internal static class Utility
    {

        internal static void CalcSizeInEmu(Image image, out long widthEmu, out long heightEmu)
        {
            //one inch is 914400 EMU
            //one inch is 1140 twips
            //one inch is 96 pixels


            double widthInches = 0;
            double heightInches = 0;

            switch (image.Metadata.ResolutionUnits)
            {
                case SixLabors.ImageSharp.Metadata.PixelResolutionUnit.AspectRatio:
                case SixLabors.ImageSharp.Metadata.PixelResolutionUnit.PixelsPerInch:
                    widthInches = (image.Width * 1d) / image.Metadata.HorizontalResolution;
                    heightInches = (image.Height * 1d) / image.Metadata.VerticalResolution;
                    break;
                case SixLabors.ImageSharp.Metadata.PixelResolutionUnit.PixelsPerCentimeter:
                    widthInches = (image.Width * 2.54) / image.Metadata.HorizontalResolution;
                    heightInches = (image.Height * 2.54) / image.Metadata.VerticalResolution;
                    break;
                case SixLabors.ImageSharp.Metadata.PixelResolutionUnit.PixelsPerMeter:
                    widthInches = (image.Width * 0.00254) / image.Metadata.HorizontalResolution;
                    heightInches = (image.Height *  0.00254) / image.Metadata.VerticalResolution;
                    break;
                default:
                    widthEmu = 0;
                    heightEmu = 0;
                    break;
            }

            widthEmu = (long)(widthInches * 914400);
            heightEmu = (long)(heightInches * 914400);

        }

        internal static Stream? TryGetJpegStreamExternal(string externalUrl, out string message, out long Height, out long Width)
        {

            Stream? output = new MemoryStream();

            message = "OK";
            Height = 0;
            Width = 0;

            try
            {
                using HttpClient client = new HttpClient();
                Task<HttpResponseMessage> responseTask = client.GetAsync(externalUrl);
                responseTask.Wait();

                using var result = responseTask.Result;
                    
                if (result.IsSuccessStatusCode)
                {
                    using Stream stream = result.Content.ReadAsStream();

                    using var image = Image.Load(stream);
                    
                    image.SaveAsJpeg(output);
                    CalcSizeInEmu(image, out Width, out Height);
                    return output;

                }
                else
                {
                    throw new Exception("url_not_found");
                }
                

            }
            catch (InvalidImageContentException)
            {
                message = "Не изображение";       
                return null;
            }
            catch (UnknownImageFormatException)
            {
                message = "Данный формат изображения не поддерживается.";
                return null;
            }
            catch (UriFormatException) {
                message = "Ссылка неверного формата";
                return null;
            }
            catch (HttpRequestException)
            {
                message = "Невозможно загрузить изображение";
                return null;
            }
            catch (Exception)
            {
                message = "Неизвестная ошибка";
                return null;
            }
            return output;
        }

        internal static Stream? TryGetJpegStreamLocal(string localUrl, out string message, out long Height, out long Width) {

            Stream? output = new MemoryStream();
            message = "OK";
            Height = 0;
            Width = 0;
            try
            {

                byte[] bytes = File.ReadAllBytes(localUrl);
                var image = Image.Load(bytes);
                image.SaveAsJpeg(output);
                CalcSizeInEmu(image, out Width, out Height);
                return output;

            }
            catch (InvalidImageContentException)
            {
                message = "Не изображение";
                return null;
            }
            catch (UnknownImageFormatException)
            {
                message = "Данный формат изображения не поддерживается.";
                return null;
            }
            catch (Exception)
            {
                message = "Ошибка чтения файла, возможно файл отсутствует";
                return null;
            }
        
        }


    }
}
