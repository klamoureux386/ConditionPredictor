from transformers import AutoModelForCausalLM, AutoTokenizer, Trainer, TrainingArguments, DataCollatorForSeq2Seq
from peft import LoraConfig, TaskType, get_peft_model
from pathlib import Path

# TO DO: import model information from AppHost appsettings.
# Note that this path is pointing towards the real (UserFolder) .cache and not the symlink AppHost .cache.
# modelPath = (
#     Path.home() /
#     ".cache" /
#     "huggingface" /
#     "hub"/
#     "models--BioMistral--BioMistral-7B-AWQ-QGS128-W4-GEMM" /
#     "snapshots" /
#     "6739b645fb6a30dd9029c06b0bb477a47736648d"
#   ).resolve()

tokenizer = AutoTokenizer.from_pretrained('BioMistral/BioMistral-7B-AWQ-QGS128-W4-GEMM')
# Reference local .cache folder from pre-loaded 
model = AutoModelForCausalLM.from_pretrained('BioMistral/BioMistral-7B-AWQ-QGS128-W4-GEMM', device_map="auto")

# Wrap the model in LoRA - MUST MATCH adapter_config.json
peft_config = LoraConfig(
  task_type=TaskType.CAUSAL_LM, inference_mode=False,
  r=16, lora_alpha=32, lora_dropout=0.01,
  target_modules=["q_proj", "k_proj", "v_proj"]
)
model = get_peft_model(model, peft_config)
model.print_trainable_parameters()

# Load the dataset
from datasets import load_dataset
ds = load_dataset("json", data_files={"train": "fake-disease.jsonl"})
def tokenize(e): return tokenizer(e["prompt"] + e["completion"], truncation=True)
ds = ds.map(tokenize, batched=True)

# Train
training_args = TrainingArguments(
  output_dir="fake-disease-adapter",
  per_device_train_batch_size=2,
  num_train_epochs=10,
  learning_rate=1e-4,
  save_strategy="epoch", load_best_model_at_end=True
)
trainer = Trainer(
  model=model, args=training_args,
  train_dataset=ds["train"],
  data_collator=DataCollatorForSeq2Seq(tokenizer, pad_to_multiple_of=8)
)
trainer.train()

# Save safely
model.save_pretrained("fake-disease-adapter", safe_serialization=True)
