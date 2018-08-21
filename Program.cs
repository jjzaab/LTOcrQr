using System;
using Leadtools;
using Leadtools.Codecs;
using Leadtools.Barcode;
using Leadtools.Ocr;
using Leadtools.ImageProcessing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace OcrToQr
{
    class Program
    {
        static void Main(string[] args)
        {
            String fileToConvert = @"FILE PATH HERE";
            RasterSupport.SetLicense(@"C:\LEADTOOLS 20\Common\License\LEADTOOLS.LIC", System.IO.File.ReadAllText(@"C:\LEADTOOLS 20\Common\License\LEADTOOLS.LIC.KEY"));

            using(RasterCodecs codecs = new RasterCodecs())
            {
                using (IOcrEngine ocrEngine = OcrEngineManager.CreateEngine(OcrEngineType.LEAD, false))
                {
                    ocrEngine.Startup(null, null, null, @"C:\LEADTOOLS 20\Bin\Common\OcrLEADRuntime");

                    using (IOcrPage ocrPage = ocrEngine.CreatePage(ocrEngine.RasterCodecsInstance.Load(fileToConvert, 1), OcrImageSharingMode.AutoDispose))
                    {
                        ocrPage.AutoZone(null);
                        ocrPage.Recognize(null);
                        string recognizedCharacters = ocrPage.GetText(-1);

                        BarcodeEngine engine = new BarcodeEngine();
                        int resolution = 300;
                        using (RasterImage image = RasterImage.Create((int)(8.5 * resolution), (int)(11.0 * resolution), 1, resolution, RasterColor.FromKnownColor(RasterKnownColor.White)))
                        {
                            BarcodeWriter writer = engine.Writer;

                            QRBarcodeData data = BarcodeData.CreateDefaultBarcodeData(BarcodeSymbology.QR) as QRBarcodeData;

                            data.Bounds = new LeadRect(0, 0, image.ImageWidth, image.ImageHeight);
                            QRBarcodeWriteOptions writeOptions = writer.GetDefaultOptions(data.Symbology) as QRBarcodeWriteOptions;
                            writeOptions.XModule = 30;
                            writeOptions.HorizontalAlignment = BarcodeAlignment.Near;
                            writeOptions.VerticalAlignment = BarcodeAlignment.Near;
                            data.Value = recognizedCharacters;

                            writer.CalculateBarcodeDataBounds(new LeadRect(0, 0, image.ImageWidth, image.ImageHeight), image.XResolution, image.YResolution, data, writeOptions);
                            Console.WriteLine("{0} by {1} pixels", data.Bounds.Width, data.Bounds.Height);

                            writer.WriteBarcode(image, data, writeOptions);

                            CropCommand cmd = new CropCommand(new LeadRect(0, 0, data.Bounds.Width, data.Bounds.Height));
                            cmd.Run(image);

                            codecs.Save(image, "QR.tif", RasterImageFormat.CcittGroup4, 1);

                            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                            {
                                var process = new Process();
                                process.StartInfo = new ProcessStartInfo("QR.tif")
                                {
                                    UseShellExecute = true
                                };
                                process.Start();
                            }

                            Console.WriteLine();
                        }
                    }
                }
            }
        }
    }
}
