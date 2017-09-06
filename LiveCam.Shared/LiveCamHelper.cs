using Microsoft.ProjectOxford.Face;
using ServiceHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveCam.Shared
{
    public static class LiveCamHelper
    {
        public static bool IsFaceRegistered { get; set; }

        public static bool IsInitialized { get; set; }

        public static string WorkspaceKey
        {
            get;
            set;
        }
        public static Action<string> GreetingsCallback { get => greetingsCallback; set => greetingsCallback = value; }

        private static Action<string> greetingsCallback;

        public static void Init(Action throttled = null)
        {
            FaceServiceHelper.ApiKey = "7b4b8a16c8e74abf9b73563e23f3518b";
            WorkspaceKey = "EnigmaMx";
            //user-provided data attached to the person group
            if (throttled!=null)
            FaceServiceHelper.Throttled += throttled;
            ImageAnalyzer.PeopleGroupsUserDataFilter = WorkspaceKey;
            FaceListManager.FaceListsUserDataFilter = WorkspaceKey;

            IsInitialized = true;
        }
        public static async Task RegisterFaces()
        {
            try
            {
                var persongroupId = Guid.NewGuid().ToString();
                // persongroupId=jpd12345678

                await FaceServiceHelper.CreatePersonGroupAsync(persongroupId,
                                                        "Xamarin",
                                                     WorkspaceKey);
                Console.WriteLine(persongroupId);
                await FaceServiceHelper.CreatePersonAsync(persongroupId, "Albert Einstein");
                var personsInGroup = await FaceServiceHelper.GetPersonsAsync(persongroupId);
                await FaceServiceHelper.AddPersonFaceAsync(persongroupId, personsInGroup[0].PersonId,
                    "https://upload.wikimedia.org/wikipedia/commons/d/d3/Albert_Einstein_Head.jpg", null, null);


                    await FaceServiceHelper.CreatePersonAsync(persongroupId, "Jorge Perales Diaz");
                    await FaceServiceHelper.AddPersonFaceAsync(persongroupId, personsInGroup[1].PersonId,
                        "https://enigmaimages.blob.core.windows.net/enigmaimages/jorge.png",
                        null, null);

                    await FaceServiceHelper.TrainPersonGroupAsync(persongroupId);

                    IsFaceRegistered = true;
            }
            catch (FaceAPIException ex)

            {
                Console.WriteLine("Error> "+ex.Message);
                IsFaceRegistered = false;
            }
        }

        public static async Task ProcessCameraCapture(ImageAnalyzer e)
        {

            DateTime start = DateTime.Now;

            await e.DetectFacesAsync();

            if (e.DetectedFaces.Any())
            {
                await e.IdentifyFacesAsync();
                string greetingsText = GetGreettingFromFaces(e);

                if (e.IdentifiedPersons.Any())
                {

                    if (greetingsCallback != null)
                    {
                        DisplayMessage(greetingsText);
                    }

                    Console.WriteLine(greetingsText);
                }
                else
                {
                    DisplayMessage("No Idea, who you're.. Register your face.");

                    Console.WriteLine("No Idea");

                }
            }
            else
            {
               // DisplayMessage("No face detected.");

                Console.WriteLine("No Face ");

            }

            TimeSpan latency = DateTime.Now - start;
            var latencyString = string.Format("Face API latency: {0}ms", (int)latency.TotalMilliseconds);
            Console.WriteLine(latencyString);
        }

        private static string GetGreettingFromFaces(ImageAnalyzer img)
        {
            if (img.IdentifiedPersons.Any())
            {
                string names = img.IdentifiedPersons.Count() > 1 ? string.Join(", ", img.IdentifiedPersons.Select(p => p.Person.Name)) : img.IdentifiedPersons.First().Person.Name;

                if (img.DetectedFaces.Count() > img.IdentifiedPersons.Count())
                {
                    return string.Format("Identified subjects: {0} and company!", names);
                }
                else
                {
                    return string.Format("Subject identified, {0}!", names);
                }
            }
            else
            {
                if (img.DetectedFaces.Count() > 1)
                {
                    return "Hi everyone! If I knew any of you by name I would say it...";
                }
                else
                {
                    return "Hi there! If I knew you by name I would say it...";
                }
            }
        }

        static void DisplayMessage(string greetingsText)
        {
            greetingsCallback?.Invoke(greetingsText);
        }
    }
}
