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
#     "models--microsoft--MediPhi-Instruct" /
#     "snapshots" /
#     "a94ac478e7c246103d55665a0804684042f3b973"
#   ).resolve()

tokenizer = AutoTokenizer.from_pretrained('microsoft/MediPhi-Instruct')
# Reference local .cache folder from pre-loaded 
model = AutoModelForCausalLM.from_pretrained('microsoft/MediPhi-Instruct', device_map="auto")

# for name, module in model.named_modules():
#     print(name)

# Wrap the model in LoRA - MUST MATCH adapter_config.json
peft_config = LoraConfig(
  task_type=TaskType.CAUSAL_LM,
  inference_mode=False,
  r=16,
  lora_alpha=32,
  lora_dropout=0.01,
  target_modules=[
    "self_attn.o_proj",
    "self_attn.qkv_proj",
    "mlp.gate_up_proj",
    "mlp.down_proj"
  ]
)
model = get_peft_model(model, peft_config)
model.print_trainable_parameters()

# Load the dataset
from datasets import load_dataset

def tokenize(batch):
  outputs = tokenizer(batch["prompt"], batch["completion"], truncation=True, padding=True)
  return {
    "input_ids": outputs["input_ids"],
    "attention_mask": outputs["attention_mask"],
    "labels": outputs["input_ids"],  # works for decoder-only models
  }

ds = load_dataset("json", data_files={"train": "fake-disease.jsonl"})
ds = ds.map(tokenize, batched=True)

# Train
training_args = TrainingArguments(
  output_dir="fake-disease-adapter",
  per_device_train_batch_size=2,
  num_train_epochs=10,
  learning_rate=1e-4,
  save_strategy="epoch",
  eval_strategy="no",       # disable evaluation
  load_best_model_at_end=False,
  #metric_for_best_model="loss"
)

trainer = Trainer(
    model=model,
    args=training_args,
    train_dataset=ds["train"],
    # eval_dataset=eval_ds,  # if using evaluation
    data_collator=DataCollatorForSeq2Seq(tokenizer, pad_to_multiple_of=8),
)
trainer.train()

model.save_pretrained("fake-disease-adapter", safe_serialization=True)
