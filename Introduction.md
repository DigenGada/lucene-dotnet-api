## Introduction ##
This Lucene.NET API has been codenamed The Hammer. It's purpose is to create a friendly, easy-to-use, intuitive wrapper around Lucene.NET 2.9.

## But why built a wrapper around Lucene? ##
I fell in love with Lucene the first time I used it but over time realized that unless you understand the big picture of what it does and how it works that its actually kind of difficult to consume. That's when I knew it was time to create a wrapper for normal everyday developers who just need to implement a search solution on the fly and can't dedicate hours to understand how and why it works. They need it to JUST WORK. If that's you, then this is your solution.

Don't get me wrong, I think the Lucene .NET library is written beautifully and it follows the wonderful mantra of "do one thing and do it well". The one thing it does is work with indexes. What this means though is that its up to the consumer to get all the necessary data into a format that can be consumed by Lucene. This library also seeks to provide some relief in that area by allowing you to more easily index and retrieve both simple and complex types of data.

## Other considerations ##
Functionality breeds complexity, and its tough to balance one with the other. At what point do you lack 'required' functionality? At what point does your library do so much that it's no longer easy to use? Those are difficult questions to answer but do my best to make sure this library walks that fine line between feature rich and simple to use.

For the most part I'm a documentation bum. I _love_ to fully document my code in line and enjoy maintaining external documentation for others to read. Since the code has just been added to this project there's not much in the way of documentation, but as time goes on I promise to make sure that there is sufficient documentation to walk you through whatever need. And if there's some random, weird, messed up use case that you run into and can't solve, then submit it to the issues section. I want to hear from you!