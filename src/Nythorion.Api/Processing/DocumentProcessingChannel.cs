using System.Threading.Channels;

namespace Nythorion.Api.Processing;

public sealed class DocumentProcessingChannel
{
    private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

    public ChannelWriter<Guid> Writer => _channel.Writer;
    public ChannelReader<Guid> Reader => _channel.Reader;
}
