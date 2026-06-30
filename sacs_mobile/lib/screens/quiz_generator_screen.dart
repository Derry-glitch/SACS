import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import '../services/api_service.dart';
import '../core/theme/app_theme.dart';

class QuizGenerationScreen extends StatefulWidget {
  const QuizGenerationScreen({super.key});

  @override
  State<QuizGenerationScreen> createState() => _QuizGenerationScreenState();
}

class _QuizGenerationScreenState extends State<QuizGenerationScreen> {
  final ApiService _apiService = ApiService();
  final _formKey = GlobalKey<FormState>();
  final _contentController = TextEditingController();

  String _selectedDifficulty = 'Medium';
  bool _isProcessing = false;
  String? _errorMessage;

  // Quiz output data
  String? _quizTitle;
  List<dynamic> _questions = [];
  Map<int, String> _selectedAnswers = {}; // map of questionIndex -> selectedOption
  bool _quizSubmitted = false;

  @override
  void dispose() {
    _contentController.dispose();
    super.dispose();
  }

  Future<void> _generateQuiz() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isProcessing = true;
      _quizTitle = null;
      _questions = [];
      _selectedAnswers = {};
      _quizSubmitted = false;
      _errorMessage = null;
    });

    try {
      final data = await _apiService.generateQuiz(_contentController.text, _selectedDifficulty);
      setState(() {
        _quizTitle = data['quizTitle'] as String? ?? 'AI Practice Quiz';
        _questions = data['questions'] as List<dynamic>? ?? [];
        _isProcessing = false;
      });
    } catch (e) {
      setState(() {
        _errorMessage = 'Failed to generate quiz: ${e.toString()}';
        _isProcessing = false;
      });
    }
  }

  int _calculateScore() {
    int score = 0;
    for (int i = 0; i < _questions.length; i++) {
      final q = _questions[i] as Map<String, dynamic>;
      final correctOpt = q['correctAnswer'] as String;
      if (_selectedAnswers[i] == correctOpt) {
        score++;
      }
    }
    return score;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(
          'AI Quiz Generator',
          style: GoogleFonts.outfit(fontWeight: FontWeight.bold),
        ),
        backgroundColor: AppTheme.bgDark,
        elevation: 0,
      ),
      body: Container(
        height: double.infinity,
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
            colors: [AppTheme.bgDark, AppTheme.bgDarkSecondary],
          ),
        ),
        child: SafeArea(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24.0),
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Generate Practice Exams',
                    style: GoogleFonts.outfit(
                      color: AppTheme.textPrimary,
                      fontSize: 22,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    'Convert any notes, lectures, or topics into interactive multiple-choice questions to reinforce your learning.',
                    style: GoogleFonts.inter(
                      color: AppTheme.textSecondary,
                      fontSize: 13,
                      height: 1.4,
                    ),
                  ),
                  const SizedBox(height: 28),

                  // Input Text area
                  Text(
                    'Lecture Notes / Study Content',
                    style: GoogleFonts.outfit(
                      color: AppTheme.textSecondary,
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                    ),
                  ),
                  const SizedBox(height: 8),
                  TextFormField(
                    controller: _contentController,
                    maxLines: 5,
                    decoration: InputDecoration(
                      hintText: 'e.g., Labeled data vs unlabeled data in machine learning slides, or relational database ACID rules...',
                      hintStyle: TextStyle(color: AppTheme.textSecondary.withOpacity(0.4)),
                      filled: true,
                      fillColor: AppTheme.bgDarkSecondary,
                      contentPadding: const EdgeInsets.all(16),
                      enabledBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(16),
                        borderSide: BorderSide(color: Colors.white.withOpacity(0.06)),
                      ),
                      focusedBorder: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(16),
                        borderSide: const BorderSide(color: AppTheme.primaryLight),
                      ),
                    ),
                    style: const TextStyle(color: AppTheme.textPrimary, height: 1.4),
                    validator: (val) => val == null || val.trim().isEmpty ? 'Please enter some study content' : null,
                  ),
                  const SizedBox(height: 16),

                  // Difficulty dropdown & generate button row
                  Row(
                    children: [
                      Expanded(
                        child: DropdownButtonFormField<String>(
                          value: _selectedDifficulty,
                          decoration: InputDecoration(
                            labelText: 'Difficulty',
                            labelStyle: const TextStyle(color: AppTheme.textSecondary),
                            filled: true,
                            fillColor: AppTheme.bgDarkSecondary,
                            enabledBorder: OutlineInputBorder(
                              borderRadius: BorderRadius.circular(12),
                              borderSide: BorderSide(color: Colors.white.withOpacity(0.06)),
                            ),
                          ),
                          dropdownColor: AppTheme.bgDarkSecondary,
                          style: const TextStyle(color: AppTheme.textPrimary),
                          items: ['Easy', 'Medium', 'Hard'].map((String level) {
                            return DropdownMenuItem<String>(
                              value: level,
                              child: Text(level),
                            );
                          }).toList(),
                          onChanged: (val) {
                            if (val != null) {
                              setState(() {
                                _selectedDifficulty = val;
                              });
                            }
                          },
                        ),
                      ),
                      const SizedBox(width: 16),
                      SizedBox(
                        height: 58,
                        child: ElevatedButton.icon(
                          onPressed: _isProcessing ? null : _generateQuiz,
                          icon: _isProcessing
                              ? const SizedBox(
                                  width: 20,
                                  height: 20,
                                  child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2),
                                )
                              : const Icon(Icons.psychology_rounded, size: 20),
                          label: Text(
                            'Generate Quiz',
                            style: GoogleFonts.outfit(fontWeight: FontWeight.bold),
                          ),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: AppTheme.accent,
                            foregroundColor: Colors.white,
                            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 32),

                  // Error Box
                  if (_errorMessage != null)
                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: AppTheme.error.withOpacity(0.1),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: AppTheme.error.withOpacity(0.3)),
                      ),
                      child: Text(
                        _errorMessage!,
                        style: const TextStyle(color: AppTheme.error, fontSize: 13),
                      ),
                    ),

                  // Display Quiz Output
                  if (_questions.isNotEmpty) ...[
                    // Quiz title card
                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: AppTheme.accent.withOpacity(0.08),
                        borderRadius: BorderRadius.circular(12),
                        border: Border.all(color: AppTheme.accent.withOpacity(0.2)),
                      ),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            _quizTitle ?? 'Practice Quiz',
                            style: GoogleFonts.outfit(
                              color: AppTheme.textPrimary,
                              fontSize: 18,
                              fontWeight: FontWeight.bold,
                            ),
                          ),
                          const SizedBox(height: 4),
                          Text(
                            'Difficulty: $_selectedDifficulty | ${_questions.length} Questions',
                            style: GoogleFonts.inter(color: AppTheme.textSecondary, fontSize: 12),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 24),

                    // Questions List
                    ListView.builder(
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      itemCount: _questions.length,
                      itemBuilder: (context, qIndex) {
                        final q = _questions[qIndex] as Map<String, dynamic>;
                        final questionText = q['questionText'] as String;
                        final options = List<String>.from(q['options'] as List<dynamic>);
                        final correctAnswer = q['correctAnswer'] as String;
                        final explanation = q['explanation'] as String?;

                        return Container(
                          margin: const EdgeInsets.only(bottom: 24),
                          padding: const EdgeInsets.all(20),
                          decoration: BoxDecoration(
                            color: AppTheme.bgDarkSecondary,
                            borderRadius: BorderRadius.circular(20),
                            border: Border.all(
                              color: _quizSubmitted
                                  ? (_selectedAnswers[qIndex] == correctAnswer
                                      ? AppTheme.success.withOpacity(0.4)
                                      : AppTheme.error.withOpacity(0.4))
                                  : Colors.white.withOpacity(0.06),
                            ),
                          ),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Text(
                                'Question ${qIndex + 1}',
                                style: GoogleFonts.outfit(
                                  color: AppTheme.accent,
                                  fontSize: 13,
                                  fontWeight: FontWeight.bold,
                                ),
                              ),
                              const SizedBox(height: 6),
                              Text(
                                questionText,
                                style: GoogleFonts.outfit(
                                  color: AppTheme.textPrimary,
                                  fontSize: 15,
                                  fontWeight: FontWeight.bold,
                                  height: 1.4,
                                ),
                              ),
                              const SizedBox(height: 16),

                              // Render options
                              ...options.map((opt) {
                                final isSelected = _selectedAnswers[qIndex] == opt;
                                Color tileColor = Colors.transparent;
                                Color borderColor = Colors.white.withOpacity(0.06);

                                if (isSelected) {
                                  tileColor = AppTheme.accent.withOpacity(0.12);
                                  borderColor = AppTheme.accent;
                                }

                                if (_quizSubmitted) {
                                  if (opt == correctAnswer) {
                                    tileColor = AppTheme.success.withOpacity(0.15);
                                    borderColor = AppTheme.success;
                                  } else if (isSelected && isSelected != (opt == correctAnswer)) {
                                    tileColor = AppTheme.error.withOpacity(0.15);
                                    borderColor = AppTheme.error;
                                  }
                                }

                                return Padding(
                                  padding: const EdgeInsets.only(bottom: 10.0),
                                  child: InkWell(
                                    onTap: _quizSubmitted
                                        ? null
                                        : () {
                                            setState(() {
                                              _selectedAnswers[qIndex] = opt;
                                            });
                                          },
                                    borderRadius: BorderRadius.circular(12),
                                    child: Container(
                                      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
                                      decoration: BoxDecoration(
                                        color: tileColor,
                                        borderRadius: BorderRadius.circular(12),
                                        border: Border.all(color: borderColor),
                                      ),
                                      child: Row(
                                        children: [
                                          Expanded(
                                            child: Text(
                                              opt,
                                              style: GoogleFonts.inter(
                                                color: AppTheme.textPrimary,
                                                fontSize: 13,
                                              ),
                                            ),
                                          ),
                                          if (isSelected && !_quizSubmitted)
                                            const Icon(Icons.check_circle, color: AppTheme.accent, size: 18),
                                          if (_quizSubmitted && opt == correctAnswer)
                                            const Icon(Icons.check_circle_rounded, color: AppTheme.success, size: 18),
                                          if (_quizSubmitted && isSelected && _selectedAnswers[qIndex] != correctAnswer)
                                            const Icon(Icons.cancel_rounded, color: AppTheme.error, size: 18),
                                        ],
                                      ),
                                    ),
                                  ),
                                );
                              }).toList(),

                              // Explanation field
                              if (_quizSubmitted && explanation != null && explanation.trim().isNotEmpty) ...[
                                const SizedBox(height: 12),
                                const Divider(color: Colors.white12),
                                const SizedBox(height: 8),
                                Text(
                                  'Explanation:',
                                  style: GoogleFonts.outfit(
                                    color: AppTheme.textPrimary,
                                    fontSize: 12,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                const SizedBox(height: 4),
                                Text(
                                  explanation,
                                  style: GoogleFonts.inter(
                                    color: AppTheme.textSecondary,
                                    fontSize: 12,
                                    height: 1.4,
                                  ),
                                ),
                              ],
                            ],
                          ),
                        );
                      },
                    ),

                    // Submit / Reset Row
                    if (!_quizSubmitted)
                      SizedBox(
                        width: double.infinity,
                        height: 52,
                        child: ElevatedButton.icon(
                          onPressed: () {
                            setState(() {
                              _quizSubmitted = true;
                            });
                          },
                          icon: const Icon(Icons.assignment_turned_in_rounded),
                          label: Text(
                            'Submit Quiz Answers',
                            style: GoogleFonts.outfit(fontWeight: FontWeight.bold),
                          ),
                          style: ElevatedButton.styleFrom(
                            backgroundColor: AppTheme.success,
                            foregroundColor: Colors.white,
                            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                          ),
                        ),
                      )
                    else ...[
                      // Score Tag Card
                      Container(
                        width: double.infinity,
                        padding: const EdgeInsets.all(20.0),
                        decoration: BoxDecoration(
                          color: AppTheme.bgDarkSecondary,
                          borderRadius: BorderRadius.circular(20),
                          border: Border.all(color: AppTheme.primaryLight, width: 1.5),
                        ),
                        child: Column(
                          children: [
                            Text(
                              'Practice Quiz Score',
                              style: GoogleFonts.outfit(color: AppTheme.textSecondary, fontSize: 13),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              '${_calculateScore()} / ${_questions.length}',
                              style: GoogleFonts.outfit(
                                color: AppTheme.textPrimary,
                                fontSize: 32,
                                fontWeight: FontWeight.bold,
                              ),
                            ),
                            const SizedBox(height: 16),
                            SizedBox(
                              width: 150,
                              height: 40,
                              child: OutlinedButton(
                                onPressed: () {
                                  setState(() {
                                    _quizSubmitted = false;
                                    _selectedAnswers = {};
                                  });
                                },
                                style: OutlinedButton.styleFrom(
                                  side: const BorderSide(color: AppTheme.primaryLight),
                                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
                                ),
                                child: Text(
                                  'Retry Quiz',
                                  style: GoogleFonts.outfit(
                                    color: AppTheme.primaryLight,
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ],
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
