import 'dart:io';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:file_picker/file_picker.dart';
import 'package:google_fonts/google_fonts.dart';

import '../state/ai_provider.dart';
import '../../../../core/theme/app_theme.dart';

class AIFeaturesScreen extends ConsumerStatefulWidget {
  const AIFeaturesScreen({super.key});

  @override
  ConsumerState<AIFeaturesScreen> createState() => _AIFeaturesScreenState();
}

class _AIFeaturesScreenState extends ConsumerState<AIFeaturesScreen> with SingleTickerProviderStateMixin {
  late TabController _tabController;

  // Summarize Fields
  File? _summarizeFile;
  int _summarizeCourseId = 1;

  // Quiz Fields
  final _quizTitleController = TextEditingController();
  final _quizContentController = TextEditingController();
  int _quizCourseId = 1;
  String _quizDifficulty = 'Medium';

  // Study Plan Fields
  final _planNameController = TextEditingController(text: 'Weekly Study Plan');
  final Map<String, double> _freeHours = {
    'Monday': 2.0,
    'Tuesday': 2.0,
    'Wednesday': 2.0,
    'Thursday': 2.0,
    'Friday': 2.0,
    'Saturday': 4.0,
    'Sunday': 4.0,
  };

  // Deadline Fields
  final _deadlineRawTextController = TextEditingController();
  String _deadlineSourceChannel = 'Telegram';

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 4, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    _quizTitleController.dispose();
    _quizContentController.dispose();
    _planNameController.dispose();
    _deadlineRawTextController.dispose();
    super.dispose();
  }

  Future<void> _pickSummarizeFile() async {
    final result = await FilePicker.platform.pickFiles(
      type: FileType.custom,
      allowedExtensions: ['pdf', 'docx', 'txt'],
    );
    if (result != null && result.files.single.path != null) {
      setState(() {
        _summarizeFile = File(result.files.single.path!);
      });
    }
  }

  void _submitSummarize() {
    if (_summarizeFile != null) {
      ref.read(aiProvider.notifier).summarizeNotes(_summarizeFile!, _summarizeCourseId);
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please select a file first.')),
      );
    }
  }

  void _submitQuiz() {
    if (_quizTitleController.text.isNotEmpty && _quizContentController.text.isNotEmpty) {
      ref.read(aiProvider.notifier).generateQuiz(
            courseOfferingId: _quizCourseId,
            title: _quizTitleController.text.trim(),
            lectureNoteContent: _quizContentController.text.trim(),
            difficultyLevel: _quizDifficulty,
          );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Title and content are required.')),
      );
    }
  }

  void _submitStudyPlan() {
    if (_planNameController.text.isNotEmpty) {
      ref.read(aiProvider.notifier).generateStudyPlan(
            name: _planNameController.text.trim(),
            availableFreeHours: _freeHours,
          );
    }
  }

  void _submitDeadline() {
    if (_deadlineRawTextController.text.isNotEmpty) {
      ref.read(aiProvider.notifier).extractDeadline(
            _deadlineRawTextController.text.trim(),
            _deadlineSourceChannel,
          );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please paste some text content first.')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final aiState = ref.watch(aiProvider);

    ref.listen(aiProvider, (previous, next) {
      if (next.isSuccess && next.resultMessage != null) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(next.resultMessage!),
            backgroundColor: AppTheme.success,
          ),
        );
        // Clear forms on success
        setState(() {
          _summarizeFile = null;
          _quizTitleController.clear();
          _quizContentController.clear();
          _deadlineRawTextController.clear();
        });
      }
      if (next.errorMessage != null) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(next.errorMessage!),
            backgroundColor: AppTheme.error,
          ),
        );
      }
    });

    return Scaffold(
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [AppTheme.bgDark, AppTheme.bgDarkSecondary],
          ),
        ),
        child: SafeArea(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // Header
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 24.0, vertical: 16.0),
                child: Row(
                  children: [
                    IconButton(
                      onPressed: () => context.go('/'),
                      icon: const Icon(Icons.arrow_back_ios_new_rounded, color: Colors.white),
                    ),
                    const SizedBox(width: 8),
                    Text(
                      'AI Intelligence',
                      style: GoogleFonts.outfit(
                        fontSize: 24,
                        fontWeight: FontWeight.bold,
                        color: Colors.white,
                      ),
                    ),
                  ],
                ),
              ),

              // TabBar
              TabBar(
                controller: _tabController,
                indicatorColor: AppTheme.accent,
                labelColor: Colors.white,
                unselectedLabelColor: AppTheme.textSecondary,
                isScrollable: true,
                tabs: const [
                  Tab(text: 'Summarizer'),
                  Tab(text: 'Quiz Gen'),
                  Tab(text: 'Study Plan'),
                  Tab(text: 'Deadline Extractor'),
                ],
              ),

              // TabBarView
              Expanded(
                child: TabBarView(
                  controller: _tabController,
                  children: [
                    _buildSummarizerTab(aiState.isLoading),
                    _buildQuizTab(aiState.isLoading),
                    _buildStudyPlanTab(aiState.isLoading),
                    _buildDeadlineTab(aiState.isLoading),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildSummarizerTab(bool isLoading) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(
            'Upload Lecture Notes',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 8),
          Text(
            'Support PDF, DOCX, or TXT notes. SACS AI will generate key concept bullet summaries.',
            style: Theme.of(context).textTheme.bodyMedium,
          ),
          const SizedBox(height: 28),

          // Course Dropdown
          DropdownButtonFormField<int>(
            value: _summarizeCourseId,
            decoration: const InputDecoration(
              labelText: 'Select Course',
              prefixIcon: Icon(Icons.class_outlined),
            ),
            items: const [
              DropdownMenuItem(value: 1, child: Text('Machine Learning (CS401)')),
              DropdownMenuItem(value: 2, child: Text('Distributed Systems (CS405)')),
              DropdownMenuItem(value: 3, child: Text('Compiler Design (CS408)')),
            ],
            onChanged: (val) {
              if (val != null) {
                setState(() {
                  _summarizeCourseId = val;
                });
              }
            },
          ),
          const SizedBox(height: 20),

          // File Picker Area
          InkWell(
            onTap: _pickSummarizeFile,
            child: Container(
              padding: const EdgeInsets.symmetric(vertical: 40, horizontal: 16),
              decoration: BoxDecoration(
                color: AppTheme.bgDarkSecondary,
                borderRadius: BorderRadius.circular(16),
                border: Border.all(
                  color: _summarizeFile != null ? AppTheme.accent : Colors.white.withOpacity(0.08),
                  style: BorderStyle.solid,
                ),
              ),
              child: Column(
                children: [
                  Icon(
                    _summarizeFile != null ? Icons.file_present_rounded : Icons.upload_file_rounded,
                    size: 48,
                    color: _summarizeFile != null ? AppTheme.accent : AppTheme.textSecondary,
                  ),
                  const SizedBox(height: 16),
                  Text(
                    _summarizeFile != null ? _summarizeFile!.path.split('/').last : 'Select PDF, DOCX, or TXT file',
                    style: const TextStyle(fontWeight: FontWeight.bold),
                    textAlign: TextAlign.center,
                  ),
                ],
              ),
            ),
          ),
          const SizedBox(height: 36),

          ElevatedButton(
            onPressed: isLoading ? null : _submitSummarize,
            style: ElevatedButton.styleFrom(
              backgroundColor: AppTheme.primaryLight,
              foregroundColor: AppTheme.textPrimary,
              padding: const EdgeInsets.symmetric(vertical: 16),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            ),
            child: isLoading
                ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2))
                : const Text('Summarize Notes', style: TextStyle(fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }

  Widget _buildQuizTab(bool isLoading) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(
            'Generate Smart Quiz',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 8),
          Text(
            'Paste your lecture note content or key highlights, and SACS will compose revision questions.',
            style: Theme.of(context).textTheme.bodyMedium,
          ),
          const SizedBox(height: 28),

          // Title
          TextFormField(
            controller: _quizTitleController,
            decoration: const InputDecoration(
              labelText: 'Quiz Title',
              prefixIcon: Icon(Icons.quiz_outlined),
            ),
          ),
          const SizedBox(height: 16),

          // Course
          DropdownButtonFormField<int>(
            value: _quizCourseId,
            decoration: const InputDecoration(
              labelText: 'Select Course',
              prefixIcon: Icon(Icons.class_outlined),
            ),
            items: const [
              DropdownMenuItem(value: 1, child: Text('Machine Learning (CS401)')),
              DropdownMenuItem(value: 2, child: Text('Distributed Systems (CS405)')),
              DropdownMenuItem(value: 3, child: Text('Compiler Design (CS408)')),
            ],
            onChanged: (val) {
              if (val != null) {
                setState(() {
                  _quizCourseId = val;
                });
              }
            },
          ),
          const SizedBox(height: 16),

          // Difficulty Selection
          DropdownButtonFormField<String>(
            value: _quizDifficulty,
            decoration: const InputDecoration(
              labelText: 'Difficulty Level',
              prefixIcon: Icon(Icons.psychology_outlined),
            ),
            items: const ['Easy', 'Medium', 'Hard']
                .map((lvl) => DropdownMenuItem(value: lvl, child: Text(lvl)))
                .toList(),
            onChanged: (val) {
              if (val != null) {
                setState(() {
                  _quizDifficulty = val;
                });
              }
            },
          ),
          const SizedBox(height: 16),

          // Content Box
          TextFormField(
            controller: _quizContentController,
            maxLines: 5,
            decoration: const InputDecoration(
              labelText: 'Lecture Note Content / Text',
              alignLabelWithHint: true,
              prefixIcon: Padding(
                padding: EdgeInsets.only(bottom: 80.0),
                child: Icon(Icons.text_fields_rounded),
              ),
            ),
          ),
          const SizedBox(height: 36),

          ElevatedButton(
            onPressed: isLoading ? null : _submitQuiz,
            style: ElevatedButton.styleFrom(
              backgroundColor: AppTheme.primaryLight,
              foregroundColor: AppTheme.textPrimary,
              padding: const EdgeInsets.symmetric(vertical: 16),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            ),
            child: isLoading
                ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2))
                : const Text('Generate Quiz', style: TextStyle(fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }

  Widget _buildStudyPlanTab(bool isLoading) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(
            'Generate Study Plan',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 8),
          Text(
            'Input your daily free hours and SACS will schedule optimized calendar study blocks.',
            style: Theme.of(context).textTheme.bodyMedium,
          ),
          const SizedBox(height: 28),

          // Name
          TextFormField(
            controller: _planNameController,
            decoration: const InputDecoration(
              labelText: 'Study Plan Name',
              prefixIcon: Icon(Icons.schedule_rounded),
            ),
          ),
          const SizedBox(height: 20),

          // Sliders for each day
          ..._freeHours.keys.map((day) {
            return Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(day, style: const TextStyle(fontWeight: FontWeight.bold)),
                    Text('${_freeHours[day]!.toStringAsFixed(1)} hours'),
                  ],
                ),
                Slider(
                  value: _freeHours[day]!,
                  min: 0,
                  max: 10,
                  divisions: 20,
                  activeColor: AppTheme.accent,
                  onChanged: (val) {
                    setState(() {
                      _freeHours[day] = val;
                    });
                  },
                ),
                const SizedBox(height: 8),
              ],
            );
          }),
          const SizedBox(height: 28),

          ElevatedButton(
            onPressed: isLoading ? null : _submitStudyPlan,
            style: ElevatedButton.styleFrom(
              backgroundColor: AppTheme.primaryLight,
              foregroundColor: AppTheme.textPrimary,
              padding: const EdgeInsets.symmetric(vertical: 16),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            ),
            child: isLoading
                ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2))
                : const Text('Generate Optimized Study Plan', style: TextStyle(fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }

  Widget _buildDeadlineTab(bool isLoading) {
    return SingleChildScrollView(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          Text(
            'Extract Deadline from Text',
            style: Theme.of(context).textTheme.titleLarge,
          ),
          const SizedBox(height: 8),
          Text(
            'Paste lecturer emails, chat forwards (Telegram, WhatsApp), or unstructured schedules. SACS will parse the deadline automatically.',
            style: Theme.of(context).textTheme.bodyMedium,
          ),
          const SizedBox(height: 28),

          // Source Channel Selector
          DropdownButtonFormField<String>(
            value: _deadlineSourceChannel,
            decoration: const InputDecoration(
              labelText: 'Source Channel',
              prefixIcon: Icon(Icons.source_outlined),
            ),
            items: const ['Telegram', 'WhatsApp', 'Email', 'LMS', 'Other']
                .map((ch) => DropdownMenuItem(value: ch, child: Text(ch)))
                .toList(),
            onChanged: (val) {
              if (val != null) {
                setState(() {
                  _deadlineSourceChannel = val;
                });
              }
            },
          ),
          const SizedBox(height: 20),

          // Raw text field
          TextFormField(
            controller: _deadlineRawTextController,
            maxLines: 6,
            decoration: const InputDecoration(
              labelText: 'Paste Unstructured Text / Messages here...',
              alignLabelWithHint: true,
              prefixIcon: Padding(
                padding: EdgeInsets.only(bottom: 100.0),
                child: Icon(Icons.paste_rounded),
              ),
            ),
          ),
          const SizedBox(height: 36),

          ElevatedButton(
            onPressed: isLoading ? null : _submitDeadline,
            style: ElevatedButton.styleFrom(
              backgroundColor: AppTheme.primaryLight,
              foregroundColor: AppTheme.textPrimary,
              padding: const EdgeInsets.symmetric(vertical: 16),
              shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            ),
            child: isLoading
                ? const SizedBox(height: 20, width: 20, child: CircularProgressIndicator(strokeWidth: 2))
                : const Text('Extract and Save Deadline', style: TextStyle(fontWeight: FontWeight.bold)),
          ),
        ],
      ),
    );
  }
}
