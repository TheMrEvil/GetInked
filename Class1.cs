using System;
using System.Collections.Generic;
using System.Reflection;
using MelonLoader;
using UnityEngine;

namespace GetInked
{
    public class Class1 : MelonMod
    {
        private bool showDropdown = false;
        private Rect windowRect = new Rect(20, 20, 400, 500); // Larger window
        private Vector2 scrollPosition = Vector2.zero;
        private readonly string[] inkOptions = new string[]
        {
            "Discordant",
            "Red_Ink",
            "Blue_Ink",
            "Pink_Ink",
            "Teal_Ink",
            "Green_Ink",
            "Orange_Ink",
            "Yellow_Ink"
        };

        public override void OnUpdate()
        {
            // Toggle menu with '-' key (both main and keypad)
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                showDropdown = !showDropdown;
            }

            // Force cursor state every frame while menu is open
            if (showDropdown)
            {
                SetCursorState(true);
            }
        }

        public override void OnGUI()
        {
            GUI.depth = 0;

            // Solid color style for the window
            GUIStyle windowStyle = new GUIStyle(GUI.skin.window);
            windowStyle.normal.background = MakeTexture(2, 2, new Color(0.12f, 0.12f, 0.12f, 1f)); // solid dark

            if (showDropdown)
            {
                windowRect = GUILayout.Window(0, windowRect, DrawWindow, "Ink Augment Manager", windowStyle);
            }
        }

        private void DrawWindow(int windowID)
        {
            // Solid color style for buttons
            GUIStyle addButtonStyle = new GUIStyle(GUI.skin.button);
            addButtonStyle.normal.background = MakeTexture(2, 2, new Color(0.2f, 0.5f, 0.2f, 1f)); // green for add
            addButtonStyle.normal.textColor = Color.white;
            addButtonStyle.fontSize = 16;
            addButtonStyle.fixedHeight = 35;

            GUIStyle removeButtonStyle = new GUIStyle(GUI.skin.button);
            removeButtonStyle.normal.background = MakeTexture(2, 2, new Color(0.5f, 0.2f, 0.2f, 1f)); // red for remove
            removeButtonStyle.normal.textColor = Color.white;
            removeButtonStyle.fontSize = 16;
            removeButtonStyle.fixedHeight = 35;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            foreach (string ink in inkOptions)
            {
                GUILayout.BeginHorizontal();

                // Add button
                if (GUILayout.Button($"Add {ink}", addButtonStyle, GUILayout.ExpandWidth(true)))
                {
                    AddInkAugment(ink);
                }

                // Remove button
                if (GUILayout.Button($"Remove {ink}", removeButtonStyle, GUILayout.ExpandWidth(true)))
                {
                    RemoveInkAugment(ink);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();

            // Make window draggable
            GUI.DragWindow();
        }

        private void AddInkAugment(string ink)
        {
            MelonLogger.Msg($"Adding ink: {ink}");

            var cmdType = typeof(Cmd_Player);
            var method = cmdType.GetMethod(
                "AddAugment",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(string), typeof(int) },
                null
            );

            if (method != null)
            {
                try
                {
                    var result = method.Invoke(null, new object[] { ink, 1 });
                    MelonLogger.Msg($"AddAugment result: {result}");
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Exception invoking AddAugment: {ex}");
                }
            }
            else
            {
                MelonLogger.Error("Could not find AddAugment method via reflection.");
            }
        }

        private void RemoveInkAugment(string ink)
        {
            MelonLogger.Msg($"Removing ink: {ink}");

            try
            {
                // Check if player exists
                if (PlayerControl.myInstance == null)
                {
                    MelonLogger.Error("Player does not exist!");
                    return;
                }

                // Get the augment tree by name
                var augmentTree = GraphDB.GetAugmentByName(ink);
                if (augmentTree == null)
                {
                    MelonLogger.Error($"Could not find augment tree for: {ink}");
                    return;
                }

                // Call RemoveAugment directly on the player instance
                PlayerControl.myInstance.RemoveAugment(augmentTree, 1);
                MelonLogger.Msg($"Successfully removed augment: {ink}");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Exception removing augment: {ex}");
            }
        }

        private void SetCursorState(bool unlocked)
        {
            if (unlocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}
