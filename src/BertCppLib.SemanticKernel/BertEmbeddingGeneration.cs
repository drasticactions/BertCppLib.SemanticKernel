using Microsoft.SemanticKernel.AI.Embeddings;

namespace BertCppLib.SemanticKernel;

public class BertEmbeddingGeneration : ITextEmbeddingGeneration
{
    private IntPtr ctx;
    BertCppInterop.bert_params bparams;
    
    public BertEmbeddingGeneration(string modelPath, BertCppInterop.bert_params bparams)
    {
        this.ctx = BertCppInterop.bert_load_from_file(modelPath);
        this.bparams = bparams;
    }
    
    public BertEmbeddingGeneration(IntPtr ctx, BertCppInterop.bert_params bparams)
    {
        this.ctx = ctx;
        this.bparams = bparams;
    }
    
    public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, CancellationToken cancellationToken = new CancellationToken())
    {
        var result = data.Select(x => BertCppInterop.bert_tokenize(ctx, x))
            .Select(x => BertCppInterop.bert_eval(ctx, bparams.n_threads, x))
            .ToList();
        
        return result.Select(n => new ReadOnlyMemory<float>(n)).ToList();
    }
}