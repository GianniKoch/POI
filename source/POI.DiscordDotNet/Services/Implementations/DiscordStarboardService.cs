﻿using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using POI.Persistence.Domain;
using POI.Persistence.Repositories;

namespace POI.DiscordDotNet.Services.Implementations;

public class DiscordStarboardService : IAddDiscordClientFunctionality
{
	private readonly ILogger<DiscordStarboardService> _logger;
	private readonly IStarboardMessageRepository _starboardMessageRepository;
	private readonly IServerSettingsRepository _serverSettingsRepository;

	public DiscordStarboardService(
		ILogger<DiscordStarboardService> logger,
		IStarboardMessageRepository starboardMessageRepository, IServerSettingsRepository serverSettingsRepository)
	{
		_logger = logger;
		_starboardMessageRepository = starboardMessageRepository;
		_serverSettingsRepository = serverSettingsRepository;
	}

	public void Setup(IDiscordClientProvider discordClientProvider)
	{
		_logger.LogDebug("Setting up DiscordStarboardService");

		var client = discordClientProvider.Client!;
		client.MessageReactionAdded -= OnReactionAdded;
		client.MessageReactionAdded += OnReactionAdded;
	}

	public void Cleanup(IDiscordClientProvider discordClientProvider)
	{
		_logger.LogDebug("Cleaning up DiscordStarboardService");
		var client = discordClientProvider.Client!;
		client.MessageReactionAdded -= OnReactionAdded;
	}

	private async Task OnReactionAdded(DiscordClient sender, MessageReactionAddEventArgs args)
	{
		//TODO: Check on emoji ID, not name
		//TODO: Figure out a way to get the display name of the user outside the guild...
		//TODO: Add self-starred LAMEEEEEEEEEEEE

		var guild = args.Guild;
		var serverSettings = await _serverSettingsRepository.FindOneById(guild.Id);
		if (serverSettings?.StarboardChannelId == null)
		{
			_logger.LogWarning("Server settings or starboard channel id not found for guild id {GuildId}!", guild.Id);
			return;
		}


		// Skip event if the message is in the starboard channel (To prevent people staring the bot messages)
		var channel = args.Channel;
		if (channel.Id == serverSettings.StarboardChannelId)
		{
			return;
		}

		// Skip event if the message is before the ignore threshold
		var message = args.Message;
		if(args.Message.CreationTimestamp < (serverSettings.StarboardMessageIgnoreAfter ?? DateTimeOffset.UtcNow))
		{
			_logger.LogInformation("Message was starred before the threshold, ignoring.");
			return;
		}

		// Check if the message reactions contains the star emote
		if (message.Reactions.All(x => x.Emoji.Name != "⭐"))
		{
			return;
		}

		// Check if the message is cached and get contents if true.
		if (message.Author == null)
		{
			message = await channel.GetMessageAsync(message.Id, true);
		}

		// Check if the message has enough stars
		var messageStarCount = message.Reactions.First(x => x.Emoji.Name == "⭐").Count;
		if (messageStarCount < serverSettings.StarboardEmojiCount)
		{
			return;
		}

		// Get the starboard channel by the server settings id
		var starboardChannel = await sender.GetChannelAsync(serverSettings.StarboardChannelId.Value);
		if (starboardChannel == null)
		{
			_logger.LogError("Starboard channel not found!");
			return;
		}

		// Get the starboard message from the database
		var foundMessage = await _starboardMessageRepository.FindOneByServerIdAndChannelIdAndMessageId(guild.Id, channel.Id, message.Id);

		// If the message is not in the database, create a new starboard message
		if (foundMessage == null)
		{
			var user = await guild.GetMemberAsync(message.Author.Id);
			if (user == null)
			{
				_logger.LogError("User with id {UserId} not found in guild {GuildId}!", message.Author.Id, guild.Id);
				return;
			}

			var embed = GetStarboardEmbed(user.DisplayName, message.Channel.Name, message.Content, message.JumpLink, message.Timestamp, (uint) messageStarCount,
				message.Attachments.FirstOrDefault()?.Url);
			var embedMessage = await starboardChannel.SendMessageAsync(embed);

			// And add to the database.
			await _starboardMessageRepository.Insert(new StarboardMessages(guild.Id, channel.Id, message.Id, embedMessage.Id));
			_logger.LogInformation("Message {JumpLink} sent to starboard channel!", message.JumpLink);
		}
		// Else the message is already in the database
		else
		{
			try
			{
				var starboardMessage = await starboardChannel.GetMessageAsync(foundMessage.StarboardMessageId);

				// Update the star count
				var embedUpdate = new DiscordEmbedBuilder(starboardMessage.Embeds[0])
					.WithFooter($"⭐{messageStarCount}")
					.Build();

				await starboardMessage.ModifyAsync(msg => msg.Embed = embedUpdate);
				_logger.LogInformation("Updated message {JumpLink} with {Stars} stars!", message.JumpLink, messageStarCount);
			}
			catch (NotFoundException)
			{
				_logger.LogWarning("{Username} starred a message that was not found in starboard channel in guild with id {GuildId}!", args.User.Username, guild.Id);
			}
		}
	}

	private static DiscordEmbed GetStarboardEmbed(string userName, string channelName, string content, Uri jumpLink, DateTimeOffset timestamp, uint messageStarCount, string? attachmentUrl = null)
	{
		var builder = new DiscordEmbedBuilder()
			.WithTitle($"{userName} in #{channelName}")
			.WithDescription(content)
			.WithColor(new DiscordColor(0x6CCAF1))
			.WithUrl(jumpLink)
			.WithFooter($"⭐{messageStarCount}")
			.WithTimestamp(timestamp);

		if (!string.IsNullOrWhiteSpace(attachmentUrl))
		{
			builder.WithImageUrl(attachmentUrl);
		}

		return builder.Build();
	}
}