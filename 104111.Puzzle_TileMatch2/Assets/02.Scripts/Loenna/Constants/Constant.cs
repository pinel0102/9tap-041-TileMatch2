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

		public const string BUTTON_CLICK_FX_NAME = "block_bubble_switch";
	}

	public static class User
	{
		public const int MAX_LIFE_COUNT = 5;
		public static readonly long REQUIRE_CHARGE_LIFE_MILLISECONDS = (long)TimeSpan.FromMinutes(30).TotalMilliseconds;
		public static readonly string DATA_PATH = Path.Combine(Application.persistentDataPath, "user.dat");
	}

	public static class Game
	{
        public const int LEVEL_PUZZLE_START = 5;
		public const float TILE_WIDTH = 88f;
		public const float TILE_HEIGHT = 92f;
        public const float TILE_WIDTH_HALF = 44f;
		public const float TILE_HEIGHT_HALF = 46f;
		public const int REQUIRED_MATCH_COUNT = 3;
		public const int MAX_BASKET_AMOUNT = 7;
		public const int STASH_TILE_AMOUNT = 3;
		public const int GOLD_PUZZLE_PIECE_COUNT = 4;
		public const float TWEEN_DURATION_SECONDS = 0.3f;
		public const float DEFAULT_DURATION_SECONDS = 0.25f;
        public const float SCALE_DURATION_SECONDS = 0.05f;

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
