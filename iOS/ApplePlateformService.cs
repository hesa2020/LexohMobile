using Foundation;
using Xamarin.Forms;
using UIKit;
using System;
using CoreGraphics;

[assembly: Dependency(typeof(Lexoh.iOS.ApplePlatformService))]
namespace Lexoh.iOS
{
    public class ApplePlatformService : IPlatformService
    {
        public void ClearWebViewCache()
        {
            NSUrlCache.SharedCache.RemoveAllCachedResponses();
        }

        public void PrintWebPage()
        {
            HybridWebViewRenderer.webView.ScrollView.ContentInset = new UIEdgeInsets(0, 0, 0, 64);

            UIPrintInteractionController printer = UIPrintInteractionController.SharedPrintController;

            printer.PrintInfo = UIPrintInfo.PrintInfo;
            printer.PrintInfo.OutputType = UIPrintInfoOutputType.General;
            printer.PrintInfo.JobName = "Lexoh";
            printer.PrintInfo.Duplex = UIPrintInfoDuplex.None;
            printer.PrintPageRenderer = new UIPrintPageRenderer();
            printer.PrintInfo.Orientation = UIPrintInfoOrientation.Portrait;

            var currentOrientation = UIApplication.SharedApplication.StatusBarOrientation;
            bool isPortrait = currentOrientation == UIInterfaceOrientation.Portrait || currentOrientation == UIInterfaceOrientation.PortraitUpsideDown;

            printer.PrintInfo.Orientation = isPortrait ? UIPrintInfoOrientation.Portrait :  UIPrintInfoOrientation.Landscape;

            var formatter = HybridWebViewRenderer.webView.ViewPrintFormatter;

            printer.PrintPageRenderer.AddPrintFormatter(formatter, 0);

            var landscapeLeft = -80;
            var portraitLeft = -30;

            formatter.ContentInsets = new UIEdgeInsets(0, isPortrait ? portraitLeft : landscapeLeft, 0, isPortrait ? portraitLeft : landscapeLeft);
            formatter.PerPageContentInsets = new UIEdgeInsets(0, isPortrait ? 0 : -15, 0, isPortrait ? 0 : -15);
            formatter.StartPage = 0;

            var renderer = printer.PrintPageRenderer;
            renderer.HeaderHeight = 0;
            renderer.FooterHeight = 0;

            //test only:
            /*
            double height, width;
            int header, sidespace;

            width = 595.2;
            height = 841.8;
            header = 0;
            sidespace = 0;

            CGSize pageSize = new CGSize(width, height);
            CGRect printableRect = new CGRect(sidespace, header, pageSize.Width - (sidespace * 2), pageSize.Height - (header * 2));
            CGRect paperRect = new CGRect(0, 0, width, height);

            renderer.SetValueForKey(NSValue.FromObject(paperRect), (NSString)"paperRect");
            renderer.SetValueForKey(NSValue.FromObject(printableRect), (NSString)"printableRect");
            */

            //formatter.MaximumContentWidth = renderer.PaperRect.Width;
            //formatter.MaximumContentHeight = renderer.PaperRect.Height;
            if (Device.Idiom == TargetIdiom.Phone)
            {
                //printer.PresentAsync(true);
                printer.PresentFromRectInView(new CGRect(0, 0, 0, 0), HybridWebViewRenderer.webView, true, (printInteractionController, completed, error) =>
                {
                    LexohPage.Instance?.ExecuteJavascript("$('.printIFrame').remove();");
                });
            }
            else if (Device.Idiom == TargetIdiom.Tablet)
            {
                printer.PresentFromRectInView(new CGRect(0, 0, 0, 0), HybridWebViewRenderer.webView, true, (printInteractionController, completed, error) => 
                {
                    LexohPage.Instance?.ExecuteJavascript("$('.printIFrame').remove();");
                });
            }
        }
    }
}
