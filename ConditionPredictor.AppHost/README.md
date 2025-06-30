The BioMistral vLLM deployment caches the weights & model on your local file system via the use of
the BindMount command below:

.WithEnvironment("HF_HOME", "/root/.cache/huggingface")
.WithEnvironment("HF_HUB_CACHE", "/root/.cache/huggingface/hub")
.WithEnvironment("VLLM_CACHE_ROOT", "/root/.cache/huggingface/vllm_cache")
.WithBindMount("/.cache/huggingface", "/root/.cache/huggingface")

This ensures that the model is only downloaded once and reused across multiple builds.