﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Alex.API.Gui;
using Alex.API.Gui.Elements;
using Alex.API.Gui.Elements.Controls;
using Alex.API.Gui.Graphics;
using Alex.API.Services;
using Alex.API.Utils;
using Alex.GameStates;
using Alex.GameStates.Gui.Common;
using Alex.Networking.Java;
using Alex.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MojangSharp.Api;
using MojangSharp.Endpoints;
using MojangSharp.Responses;

namespace Alex.Gamestates.Login
{
	public class JavaLoginState : BaseLoginState
	{
		private IPlayerProfileService _playerProfileService;
		public JavaLoginState() : base("Mojang Login")
		{
			
		}

		protected override void Initialized()
		{
			var activeProfile = Alex.ProfileManager.ActiveProfile?.Profile;
			if (activeProfile != null)
			{
				DisableInput();

				Requester.ClientToken = activeProfile.ClientToken;
				LoginButton.Enabled = false;

				NameInput.Value = activeProfile.Username;

				ErrorMessage.Text = "Validating authentication token...";
				Validate(activeProfile.AccessToken);
			}
			_playerProfileService = Alex.Services.GetService<IPlayerProfileService>();
			_playerProfileService.Authenticate += PlayerProfileServiceOnAuthenticate;
		}

		private void PlayerProfileServiceOnAuthenticate(object sender, PlayerProfileAuthenticateEventArgs e)
		{
			if (e.IsSuccess)
			{
				Alex.ProfileManager.CreateOrUpdateProfile(ProfileManager.ProfileType.Java, e.Profile, true);
				//Alex.SaveJava(_nameInput.Value);
				Alex.GameStateManager.SetActiveState("serverlist");
			}
			else
			{
				ErrorMessage.Text      = "Could not login: " + e.ErrorMessage;
				ErrorMessage.TextColor = TextColor.Red;

				LoginButton.Enabled = true;
			}
		}

		protected override void LOginButtonPressed(string username, string password)
		{
			_playerProfileService.TryAuthenticateAsync(NameInput.Value, PasswordInput.Value);
		}

		private void Validate(string accessToken)
		{
			Validate validate = new Validate(accessToken);
			validate.PerformRequestAsync().ContinueWith(task =>
			{
				var r = task.Result;
				if (r.IsSuccess)
				{
					Alex.GameStateManager.SetActiveState("serverlist");
				}
				else
				{
					ErrorMessage.Text = "Could not login: " + r.Error.ErrorMessage;
					ErrorMessage.TextColor = TextColor.Red;
				}

				EnableInput();
			});
		}
	}
}
