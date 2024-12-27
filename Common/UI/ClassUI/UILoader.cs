﻿using Terraria.ModLoader;
using Terraria.UI;
using Terraria;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Reverie.Common.UI.ClassUI
{
    internal sealed class UILoader : ILoadable
    {
        public float Priority => 1f;

        public bool LoadOnDedServer => false;

        public static List<SmartUIState> UIStates = [];
        public static List<UserInterface> UserInterfaces = [];

        public void Load(Mod reverie)
        {
            foreach (Type t in reverie.Code.GetTypes())
            {
                if (t.IsSubclassOf(typeof(SmartUIState)))
                {
                    var userInterface = new UserInterface();
                    var state = (SmartUIState)Activator.CreateInstance(t, null);

                    userInterface.SetState(state);

                    UIStates.Add(state);
                    UserInterfaces.Add(userInterface);
                }
            }
        }

        public void Unload()
        {
            UIStates?.Clear();
            UserInterfaces?.Clear();
        }

        internal static void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            for (int i = 0; i < UIStates.Count; i++)
            {
                var state = UIStates[i];
                AddLayer(layers, UserInterfaces[i], state, state.InsertionIndex(layers), state.Visible, state.Scale);
            }
        }

        public static void AddLayer(List<GameInterfaceLayer> layers, UserInterface userInterface, UIState state, int index, bool visible, InterfaceScaleType scale)
        {
            string name = state == null ? "Unknown" : state.GetType().Name;

            layers.Insert(index, new LegacyGameInterfaceLayer(Reverie.AbbreviationPrefix + name,
                delegate
                {
                    if (visible)
                    {
                        userInterface.Update(Main._drawInterfaceGameTime);
                        state.Draw(Main.spriteBatch);
                    }
                    return true;
                },
                scale
            ));
        }

        public static T GetUIState<T>() where T : SmartUIState => UIStates.FirstOrDefault(n => n is T) as T;

        public static UserInterface GetUserInterface<T>() where T : SmartUIState
        {
            int index = UIStates.IndexOf(GetUIState<T>());

            if (index == -1)
                return null;

            return UserInterfaces[index];
        }

        public static void ReloadState<T>() where T : SmartUIState
        {
            int index = UIStates.IndexOf(GetUIState<T>());
            UIStates[index] = (T)Activator.CreateInstance(typeof(T), null);

            UserInterfaces[index] = new UserInterface();
            UserInterfaces[index].SetState(UIStates[index]);
        }
    }
}