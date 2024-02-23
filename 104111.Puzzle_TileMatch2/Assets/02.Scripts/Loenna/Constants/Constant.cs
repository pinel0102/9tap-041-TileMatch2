using UnityEngine;

using System.IO;
using System;
using System.Collections.Generic;

public static class Constant
{
	public static class UI
	{
		public const float REFERENCE_RESOLUTION_X = 720f;
		public const float REFERENCE_RESOLUTION_Y = 1280f;

        public static readonly Color COLOR_WHITE = Color.white;
        public static readonly Color COLOR_BOOSTER_TIME = Color.yellow;
	}

    public static class Sound
    {
        public const string BGM = "bgm"; // 배경 음악.
        public const string SFX_BUTTON = "click_button"; // 버튼음.
        public const string SFX_TILE_SELECT = "click_object"; // 타일 선택시.
        public const string SFX_TILE_MOVE = "move_object"; // 타일 이동시.
        public const string SFX_TILE_MATCH = "match_object"; // 매칭 될 때.
        public const string SFX_TILE_MATCH_FINISH = "match_finish"; // 마지막 매칭 될 때.
        public const string SFX_ITEM_SHUFFLE = "item_shuffle"; // 아이템 - 셔플.
        public const string SFX_ITEM_UNDO = "item_undo"; // 아이템 - 언두.
        public const string SFX_ITEM_STASH = "click_object"; // 아이템 - 스태시.
        public const string SFX_REWARD_OPEN = "reward_open"; // 리워드 열릴 때.
        public const string SFX_PROGRESS = "Progress Complete"; // 수집 이벤트 프로그레스.

        //Temp
        public const string SFX_GOLD_PIECE = "popup_open"; // 골든 타일 획득시.
        public const string SFX_PUZZLE_COMPLETE = "chest_reward"; // 직소 퍼즐 완성시.
    }

	public static class User
	{
        public const int MIN_OPENLEVEL_WEEKEND_BUNDLE = 2; // 오픈시 해금.
        public const int MIN_OPENLEVEL_BEGINNER_BUNDLE = 10; // 오픈시 해금.
        public const int MIN_OPENLEVEL_DAILY_REWARD = 15; // 오픈시 해금.
        public const int MIN_OPENLEVEL_EVENT_SWEETHOLIC = 21; // 오픈시 해금.
		public const int MAX_LIFE_COUNT = 5;
        public const int MAX_LIFE_COIN = 250;
        public const int LIFE_CHARGE_TIME_MINUTES = 30;
        public const string MAX_LIFE_TEXT = "Full";
		public static readonly long REQUIRE_CHARGE_LIFE_MILLISECONDS = (long)TimeSpan.FromMinutes(LIFE_CHARGE_TIME_MINUTES).TotalMilliseconds;
		public static readonly string DATA_PATH = Path.Combine(Application.persistentDataPath, "user.dat");
	}

	public static class Game
	{
        public const int LEVEL_PUZZLE_START = 4; // 오픈시 해금.
		public const float TILE_WIDTH = 88f;
		public const float TILE_HEIGHT = 92f;
        public const float TILE_WIDTH_HALF = 44f;
		public const float TILE_HEIGHT_HALF = 46f;
		public const int REQUIRED_MATCH_COUNT = 3;
		public const int MAX_BASKET_AMOUNT = 7;
		public const int STASH_TILE_AMOUNT = 3;
		public const int GOLD_PUZZLE_PIECE_COUNT = 4;
		public const float TWEENTIME_TILE_DEFAULT = 0.15f;
        public const float TWEENTIME_TILE_SCALE = 0.05f;
        public const float TWEENTIME_JIGSAW_MOVE = 0.5f;
        public const float EFFECTTIME_TILE_MATCH = 0.3f;

		public static readonly Vector2 RESIZE_TILE_RATIOS = new Vector2(0.55f, 0.575f);

        public const float AROUND_TILE_OFFSET_LEFT = -15;
        public const float AROUND_TILE_OFFSET_RIGHT = 15;
        public const float AROUND_TILE_OFFSET_BOTTOM = -25;
        public const float AROUND_TILE_OFFSET_TOP = 15;
        public static readonly List<Vector2> AROUND_TILE_POSITION = new List<Vector2>()
        {
            new Vector2(TILE_WIDTH, TILE_HEIGHT),
            new Vector2(TILE_WIDTH, TILE_HEIGHT_HALF),
            new Vector2(TILE_WIDTH, 0),
            new Vector2(TILE_WIDTH, -TILE_HEIGHT_HALF),
            new Vector2(TILE_WIDTH, -TILE_HEIGHT),
            new Vector2(TILE_WIDTH_HALF, TILE_HEIGHT),
            new Vector2(TILE_WIDTH_HALF, TILE_HEIGHT_HALF),
            new Vector2(TILE_WIDTH_HALF, 0),
            new Vector2(TILE_WIDTH_HALF, -TILE_HEIGHT_HALF),
            new Vector2(TILE_WIDTH_HALF, -TILE_HEIGHT),
            new Vector2(-TILE_WIDTH, TILE_HEIGHT),
            new Vector2(-TILE_WIDTH, TILE_HEIGHT_HALF),
            new Vector2(-TILE_WIDTH, 0),
            new Vector2(-TILE_WIDTH, -TILE_HEIGHT_HALF),
            new Vector2(-TILE_WIDTH, -TILE_HEIGHT),
            new Vector2(-TILE_WIDTH_HALF, TILE_HEIGHT),
            new Vector2(-TILE_WIDTH_HALF, TILE_HEIGHT_HALF),
            new Vector2(-TILE_WIDTH_HALF, 0),
            new Vector2(-TILE_WIDTH_HALF, -TILE_HEIGHT_HALF),
            new Vector2(-TILE_WIDTH_HALF, -TILE_HEIGHT),
            new Vector2(0, TILE_HEIGHT),
            new Vector2(0, TILE_HEIGHT_HALF),
            new Vector2(0, -TILE_HEIGHT_HALF),
            new Vector2(0, -TILE_HEIGHT)
        };
	}

	public static class Puzzle
	{
		public const int MAX_ROW_COUNT = 5;
		public const int MAX_COLUMN_COUNT = 5;
	}

	public static class Editor
	{
		public const string CLIENT_PATH_KEY = "client_path";
		public const string DATA_PATH_KEY = "data_path";
		public const string LATEST_LEVEL_KEY = "latest_level";
		public const string DEVELOP_MODE_SCENE_KEY = "develop_mode_scene";
		public const string DEVELOP_MODE_SWITCH_KEY = "develop_mode_switch";
	}

	public static class Input
	{
		public const float DOUBLE_CLICK_SECOND = 3f;
	}

	public static class Scene
	{
		public const string CLIENT = "Game";
		public const string EDITOR = "Editor";
	}
}
