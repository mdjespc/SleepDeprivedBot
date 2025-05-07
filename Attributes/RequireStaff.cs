using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace InteractionFramework.Attributes;

public class RequireStaffAttribute : PreconditionAttribute{

    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo command, IServiceProvider services)
    {
        var application = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
        var _ownerId = application.Owner.Id;
        var user = (IGuildUser) context.User;
        
        if (user.GuildPermissions.Administrator || user.GuildPermissions.ModerateMembers || user.Id == _ownerId)
            return PreconditionResult.FromSuccess();

        return PreconditionResult.FromError("You must be an admin or the bot owner.");
    }
}
