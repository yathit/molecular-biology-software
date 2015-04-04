# Biocsharp molecular-biology-software

The Biocsharp is an open source C# tools for bioinformatics, genomics and life science research.

## Introduction

After Sangar release sequencing machine in 1975, biology start getting in touch into computational feild. Any specie on earch can be describe a four alphabets (ATCG) called nucleoides. For example human is made up of over four billions ATCG chars. Any people on earch has different sequences but almost similar because we get random combination of father and mother's sequences. Among four billion of sequences of human genome, only five percents are known to be used, while the rest of the sequence are possibily junk. Alought monkey looks a lot different from human, in genomic level, the different between the two is only 98%. Analysing these seequence among species is very useful to biology. The problem is it these sequence data are huge. If you haven't seen DNA sequences (... .faa.gz), please download one of the file from a public database here. It doesn't make sense just looking at the ATCG sequence, thought looks cool in the movie 'Matrix'. A powerful visualizing tools is require to make sense out of these huge sequence. In addition to DNA, another component of cell is protein. Unlike DNA sequence, protein sequence is make up of 20 amino acids represented as GPAVLIMCFYWHKRQNEDST. Any three DNA neuclotide has equivalent one amino acid. For example GGC in DNA sequence is G in protein sequence, CCT is equivalent to P and so on. A cell use DNA sequence for storing perment information (like harddrive) and protein sequence for operational usage (RAM). Before comversion from DNA to protein, a cell need to convert DNA to RNA and then RNA to protein. RNA is the same as DNA code except, T becomes U. There is four character in RNA. That is all in cell and the rest are small chemical molecule like water, ketone, ions, etc. You might heard of enzyme, haemoglobin, chromosome etc, all these are nothing more than a protein or a group of proteins and DNA. Since understanding life in molecular level is huge task even for big country. Most project are carried out in coloboration with internation effort. One famus project is 'Human Genome Project' finished on 2002. As the name suggest, it is sequence of human. My boss, Todd Taylor is main PI for sequence chromosome 21. People think that if we knew all the sequence, we will know the life is made of. But it was not. There is no new drug or medical improvement after knowing the human genome. The DNA sequences are so encryptic is researcher are just starting to understand. Viualization of these data is one of the key requirement for daily use for biologist. Key bioinformatics algorithm

There are several handard of computation technique (algorithm) for comparison of multiple sequences of different species, categorizing, clustering, correlation with other observed data. You can find most of these algorithms in NCBI C++ toolkits, Bioperl and Biopython. There are few others, but mainly these three cover all algorithms used by the molecular biologist. You will see there are few GUI components. They are commercial! Good for us, right? We don't put all of these algorithm into our software. In fact about 5 % of them, i think. I will give the detial later.

## Definitions, Acronyms and Abbreviation

The following are some of the terms used in the document and their definitions

Nucleotide: Represented by each of the characters ‘A’, ‘T’, ‘G’ and ‘C’. DNA Sequence: A string of nucleotides.

Genome: The entire DNA sequence of an organism.

Protein: A sequence of amino acids, a macromolecule.

Amino acid: Represented by each of the characters, 'F', 'L', 'I', 'M', 'V', 'S', 'P', 'T', 'A', 'Y', 'H', 'Q', 'N', 'K', 'D' 'E', 'C', 'W', 'R', 'G'.

Protein sequence: A string of amino acids.

## Platform and Technology

Discussion

This is most important decision. First decision is whether we do multi platform framework using java or not. Borland CLX platform has gone. I assume Qt/GTK/Tcl are powerful enough for our case. I use Java swing API for Cellware cross platform application for two year. Writing fast responsive require very careful, paintaking optimization. To get native plotfrom look and feel is tough and require extensive fine tuning and often conflict logic form one platform to another. Most of algorithm are implemented in C++ and native invocation will be require anyways. Basically i don't think we can develop quality software within two year using Java. So we will use the best framework for each platform. .NET for windows and cocca for OS X. We don't care about Linux. We left Linux platform for commmunity development work. Because it is difficult to convince linux guy to 3,000 fanncy software (yes basically practically our software non-GUI functionality can be obtained by open soure). They are fine with free scripting. Also targeting in many Linux destro cannot get revenue per effort. We will first target at Windows platform because it is more commonly used and have largest market of the three main platform. We will port to OS X at some point after getting return on investment. Another point is should be also target for web application. It is possible to develop user interaction on the web as web 2.0 are looming in few year down the line. But I think we do not have enough men power for web application development. Web application require more intensive support then web application and people are also not feel confortable with paying for web application then desktop application. Please note we have Server Edition which provide data access and reporting to client as well as on the web pages.

## Software Requirement Specification

Use case A: Sequence creation, editing and annotation
Use case A11: Vector Sequence creating from (sequence+annotation)
It is common that vendor (such as invitrogen) provide sequence in plain text and its annotation in pdf file or other text. User want to build the information into PlasmidRecord? and later it is use for visualization or other manupulation. User get the sequence (usually from the web. eg: http://www.clontech.com/images/pt/PT3715-SEQ.txt) User get its additional iniformation (usually from catelog or web. eg: http://www.clontech.com/images/pt/PT3715-5.pdf ) Create new sequence (screenshoot ) Save the file for later use

Use case A12: Vector sequence creating from NCBI database
Use know vector accession number and want to get into our software
Search in our search dialogbox entering accession number (Eg. NC_000913) Search result hits are display User choose the correct one by clicking The selected GenBank? data from NCBI database are downloaded Display sequence User may save the file

Use case A21: Copying
User select a portion of sequence By mouse drag By clicking to a annotation Copy it User may paste Other text receiving application Creat new PlasmidRecord? Paste to existing PlasmidRecord?

Use case A31: Sequence annotation
This is continuation from Use case A11, however may process from at any time while user designing the plasmid User get its annotation (usually from catelog or web. eg: http://www.clontech.com/images/pt/PT3715-5.pdf ) Add annotations (screenshoot1, screenshoot2 ) User may save the update

Use case A32: Sequence annotation edition
User select the annotation Edit annotation User may save the update User may undo the changes Use case A33: Sequence annotation deletion

User select the annotation Delete the selected annotation User may undo delete

Use case A41: Insert Restriction Sites
User select a region Commend to insert restriction sites (one of the following) Insert Restriction sites after the selection Insert Restriction sites before the selection Insert Restriction site inside the selection Insert Restriction site outside the selection
WPF framework
WPF is newer Windows UI development framework starting. I personally think Microsft new windows development framework WPF is good choice. MFC is outdated and Winform is OK but subtle different look and feel of areo. One reason for choosing WPF is its graphic render process and ability to get DPI indepedent graphic. It is extremely optimze using hardware acceleration and the framework provide effective implimation. However, I far I find, WPF is very slow as number of UI components increase in the graphic canvas. Horrible. The problem with WPF framework is luck of some component specifically multi document feature, datagrid, property editor and undoing manager. I bought Sanddock for multi document support. For datagrid, xceed has free package. We can use Winform property editor. Undoing feature will be tough. However we should aware that WPF framework is not well established framework. WCF workflow is also planed since it is very appriorate with Bioinformatic workflow. See Taverna for detail. .NET 3.5

We will use latest version of .NET framework 3.5. Data query will be implement using LINQ syntex. LINQ

All implimentation must use LINQ syntex whenever appriorate, especially Math and Database module. C# 3.0

C# 3.0 is preferred language. High performance computation

High computational recource is common with bioinformatic algorith. All bioinformatic will run in the background with custom queue system. We course gain parallel executation in the queue system. Fine grain optimization is likely to do using Intel compiler. I expect standard Grid-based distributed execution will be support in WCF. Implementation

All UI modules will be manage .NET assemble implemented in C# 3.0 language. Most bioinformatic algorithm are available as C++ or unfrequently in C. We will not attempt to convert C#, but compile as following: Use VC++ compiler to compile as manage assembly If we could not get manage assembly, we use unmanage but safe code (without raw pointer) If not, use CLI it-just-work technology to generate .NET assembly If not, compile as static link library (32 and 64 bit) and write VC++ manage wrapper If not, compile as dynamic link library (32 and 64 bit) and write VC++ manage wrapper Compiles as COM is an option but not preferred Python and perl code need to convert manually to C#. But so far i found all available in C/C++ alternative. biocsharp C# module (written from scratch for bioinofrmatic fundenmentals) is inspire by biopython module. There is no java code in bioinformatics except some bioinformatic web service use.

Peptide sequence: A protein sequence of specified length.

N-mer: A DNA sequence of length 'N'. Codon: An N-mer of size 3. Each codon represents an amino acid. Different codons can represent the same amino acid. The codons, coding for the start of the coding region are called as start codons, while the codons coding for the termination of the coding region are called as stop codons.

Genetic Code: Table containing the 64 codons, along with the amino acids they code for. The genetic code is specific to an organism.

Genes: Parts of a genome that can be biologically converted to a protein sequence are called genes. Start of a gene can be identified by presence of start codons. End of a gene can be identified by presence of stop codons. In prokaryotes, the entire region between the start codon and the stop codon is converted into a protein sequence. In eukaryotes, some regions between the start codon and stop codon cannot be converted into a protein sequence. Such parts are called introns, while the others are called exons. Genes in eukaryotes will consist of the concatenated exons sequences. Genes are also called coding regions.

Training sets: The set of known genes or protein sequences constitute the training sets to methods, using probability models such as IMMs and Mixed Memory Markov Models. They also constitute the databases used as inputs to BLAST.

GC Content: The percentage of 'G' and 'C' nucleotides in the DNA sequence.

Repeats: DNA or protein sub-sequences which occur with a certain periodicity in input DNA or protein sequences.

E-Value: Expected number of non-homologous sequences with scores greater than or equal to score x, in a database of ‘n’ sequences. Abbreviations and Acronyms

The following abbreviations and Acronyms have been used in this document. DNA : Deoxyribonucleic Acid RNA : Ribonucleic Acid BLAST : Basic Local Alignment Search Tool IUB : International Union of Biochemistry 4M : Mixed Memory Markov Model PFAM : Protein Family HMM : Hidden Markov Model IMM : Interpolated Markov Model COG : Cluster of Orthologous Genes PDB : Protein Data Bank

Molecular biology software (MoBio?) will be developed as a General purpose software for molecular biologist suitable for visualization of vector sequence, in silico manipulate of plasmids and sequence analysis. MoBio? consists of the following 13 packages in four categories. All package are managed .NET assembly. All new package will be written in C# 3.0. Package containing legacy C/C++ files are compiled under CLI manage assembly.

Utility Packages
These packages provide basic utilities functions. BaseLibrary? - Common resource, application and user setting MathLib? - Mathmetical library Plot - Charting and Plotting (Swordfish.WPF.Charts)

General Bioinformatic Packages
BioCSharp - Basic computational molecular biology BioCSharp.Algo - Bioinformatics algorithm BioCSharp.Assembly - Sequence assembly algorithm BioCSharp.Db - Bioinformatics Database API

Specific Bioinformatics Packages
Muscle - Multiple sequence alignment (C++/CLI) Primer - Primer design NcbiWrapper? - NCBI C++ toolkit wrapper (C/C++ DLL wrapper)

User Interface packages
MoBio? - Main Windows consisting five panels and workspace MoBio?.Contorls - Custom controls for specfic purpose MoBio?.PlasmidCanvas? - Plasmid visualization

System Requirements
The first development of the software is target for Microsoft Windows Vista. However it should be usable in Microsoft Windows XP SP2, Windows Server 2003 and Windows Server 2008. Windows older than XP are not supported. Hardware requirement

Dual core CPU 2 GB RAM (minimum), 4 GB recommended High speed always on internet access

Software requirement

Microsoft SQL Server Express (will be bundled with our software) Microsoft .NET Framework 3.5 (will be bundled with our software)

User Characteristics
End-Users of MoBio? are expected to be the following BS, MS or PhD-level biologists located at various research/academic institutions or organizations. Scientists with several years of research experience in the field of Biology and Bioinformatics

Besides the end users mentioned above, MoBio? is also expected to be used by system administrators who will administer update the databases/tools required for effective use of MoBio? software.
