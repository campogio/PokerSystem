using RAGE;
using RAGE.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using WiredPlayers_Client.chat;

namespace WiredPlayers_Client.globals
{
    public class Browser : Events.Script
    {
        public static string CurrentLanguage;
        public static HtmlWindow CustomBrowser;

        private static object[] Parameters;
        private static string ClosingEvent;

        public Browser()
        {
            Events.Add("destroyBrowser", DestroyBrowserEvent);
            
            Events.OnBrowserDomReady += OnBrowserDomReady;
        }

        public static void CreateBrowser(string html, string closingFunction, params object[] args)
        {
            if (CustomBrowser == null)
            {
                // Get the closing function
                ClosingEvent = closingFunction;

                // Save the rest of the parameters
                Parameters = args;

                // Create the browser
                CustomBrowser = new HtmlWindow("package://statics/html/" + html);
            }
        }

        public static void ExecuteFunction(string function, params object[] args)
        {
            // Call the function with the parameters
            CustomBrowser.Call(function, args);
        }

        public static void ForceBrowserClose()
        {
            if (ClosingEvent != null && ClosingEvent.Length > 0)
            {
                // Call the close function
                Events.CallLocal(ClosingEvent);
                ClosingEvent = string.Empty;
            }
        }

        public static void DestroyBrowserEvent(object[] args)
        {
            if (!ChatManager.Opened)
            {
                // Disable the cursor and enable the chat
                Cursor.Visible = false;
            }

            // Destroy the browser
            CustomBrowser.Destroy();
            CustomBrowser = null;
        }

        private static void OnBrowserDomReady(HtmlWindow window)
        {
            if (CustomBrowser != null && window.Id == CustomBrowser.Id && Parameters != null)
            {
                // Enable the cursor and disable the chat
                Cursor.Visible = true;

                // Add the language to the parameters
                List<object> parametersList = Parameters.ToList();
                parametersList.Insert(0, CurrentLanguage);

                // Remove the parameters
                Parameters = null;

                // Call the function passed as parameter
                ExecuteFunction("initializeMessages", parametersList.ToArray());
            }
        }        
    }
}
