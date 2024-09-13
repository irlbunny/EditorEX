using System.Reflection;

namespace EditorEX.UI
{
    internal class TributesCreditsViewController : CustomBeatmapEditorViewController
    {
        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                CreateTextGameObject("TributesHeaderText", "<size=150%>Tributes:", new(-780f, 480f), new(200f, 80f));

                // Lily :(
                var lilyImage = BeatSaberMarkupLanguage.Utilities.GetResource(Assembly.GetExecutingAssembly(), "EditorEX.Resources.lily.jpg");
                CreateImageGameObject("LilyImage", BeatSaberMarkupLanguage.Utilities.LoadSpriteRaw(lilyImage), new(-750f, 300f), new(256f, 256f));
                CreateTextGameObject("LilyHeaderText", "<size=125%>Lily/Lone/Lonely/LonelyCen", new(-375f, 375f), new(450f, 50f));
                CreateTextGameObject("LilyTributeText",
                    "<size=60%>Lily was an amazing lighter, but not only that, an amazing friend as well. She was honestly one of the nicest people I had ever met, if I had the chance to meet her again, I would do so again and again.\n" +
                    "She was honestly the most creative person in the Beat Saber community, I hope that she will now be resting happily forever. I hope we meet again sometime, thank you for being my friend, Lily! -Kaitlyn\n\n" +
                    "Rest In Peace, Lily. I (and many others) will miss you. 2001-2022",
                    new(400f, 5f), new(2000f, 650f));

                CreateTextGameObject("CreditsHeaderText", "<size=75%>Thank you, to everyone who has contributed to EditorEX!", new(-580f, 90f), new(600f, 80f));
            }
        }
    }
}
