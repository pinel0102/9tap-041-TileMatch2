namespace NineTap.Constant
{
	public static class Text
	{
		public static class Button
		{
			public const string PLAY = "Play";
			public const string PLAY_ON = "Play On";
			public const string GIVE_UP = "Give up";
			public const string CONTINUE = "Continue";
			public const string HOME = "Home";
			public const string CLAIM = "Claim";
			public const string REPLAY = "Replay";
			public const string QUIT = "Quit";
            public const string NO_THANKS = "No Thanks";
            public const string SURE = "Sure";
            public const string OK = "OK";
		}

		public static class Popup
		{
			public static class Title
			{
				public const string QUIT = "Quit Game?";
				public const string GIVE_UP = "Are you sure?";
                public const string REVIEW = "Having Fun?";
                public const string GET_STARS = "Get Stars";
			}

			public static class Message
			{
				public const string Quit = "Are you sure?\nDo you want to\nquit game?";
				public const string GIVE_UP = "You lose one life";
                public const string REVIEW_1 = "Are you enjoying\nTile Match! World Tours?";
                public const string REVIEW_2 = "Thanks for your love!";
                public const string GET_STARS = "Beat levels to earn stars!";
			}
		}

		public const string LOCKED = "Locked";
		public const string PAUSE = "Pause";
		public const string READY_POPUP_MESSAGE = "Match All Tiles!";
		public const string PLAY_END_POPUP_MESSAGE = "Press and hold to view the board";

		public static string LevelText(int level) => $"Level {level}";
		public static string LevelModeText(bool hardMode) => hardMode? "Hard" : "Normal";
		public static string GameEndLabelText(bool clear) => clear? "Level Complete!" : "No Space";
	}
}
