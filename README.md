## Demo branches

1) `main` - the initial "cut" of the demo, where we just use LM Studio (or Ollama) to chat with the phi-3 model
2) `step-2` - the next step, where we start using the AI to gather intent of a request, and then manually invoke functions to do our bidding
3) `step-3` - the final step (for now!), where we switch to using Azure Open AI with "automatic invoking" of the functions (plugins), together with a planner to orchestrate it all

## Helpful links

Here are some helpful links for learning more about Phi-3, Semantic Kernal, plugins etc

- [Phi-3 release announcement](https://azure.microsoft.com/en-us/blog/introducing-phi-3-redefining-whats-possible-with-slms?wt.mc_id=MVP_383786)

- [Phi-3-mini on Hugging Face](https://huggingface.co/microsoft/Phi-3-mini-4k-instruct)

- [What is Semantic Kernal - on Microsoft Learn](https://learn.microsoft.com/en-us/semantic-kernel/overview/?tabs=Csharp&wt.mc_id=MVP_383786)

and then within that whole Learn section, the following are particularly useful articles, including code examples that you can build to get going quickly:

- [What is an agent?](https://learn.microsoft.com/en-us/semantic-kernel/agents/?source=recommendations?wt.mc_id=MVP_383786)

- [Prompting AI models with Semantic Kernal](https://learn.microsoft.com/en-us/semantic-kernel/prompts/your-first-prompt?tabs=Csharp&wt.mc_id=MVP_383786)

- [Creating native functions for AI to call](https://learn.microsoft.com/en-us/semantic-kernel/agents/plugins/using-the-kernelfunction-decorator?wt.mc_id=MVP_383786)

And the ultimage goal...

- [Automatically orchestrate AI with Planners](https://learn.microsoft.com/en-us/semantic-kernel/agents/planners/?wt.mc_id=MVP_383786)
