import 'package:flutter/material.dart';
import 'package:intl/intl.dart';
import 'package:google_fonts/google_fonts.dart';
import '../core/theme/app_theme.dart';

class CalendarPreviewWidget extends StatefulWidget {
  const CalendarPreviewWidget({super.key});

  @override
  State<CalendarPreviewWidget> createState() => _CalendarPreviewWidgetState();
}

class _CalendarPreviewWidgetState extends State<CalendarPreviewWidget> {
  int _selectedOffset = 0; // Highlight today by default

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 90,
      child: ListView.builder(
        scrollDirection: Axis.horizontal,
        itemCount: 7,
        itemBuilder: (context, index) {
          final date = DateTime.now().add(Duration(days: index));
          final isSelected = index == _selectedOffset;

          return GestureDetector(
            onTap: () {
              setState(() {
                _selectedOffset = index;
              });
            },
            child: Container(
              width: 65,
              margin: const EdgeInsets.only(right: 12, top: 4, bottom: 4),
              decoration: BoxDecoration(
                color: isSelected ? AppTheme.primaryLight : AppTheme.bgDarkSecondary,
                borderRadius: BorderRadius.circular(16),
                boxShadow: isSelected
                    ? [
                        BoxShadow(
                          color: AppTheme.primaryLight.withOpacity(0.3),
                          blurRadius: 8,
                          offset: const Offset(0, 4),
                        ),
                      ]
                    : null,
                border: Border.all(
                  color: isSelected
                      ? AppTheme.accent.withOpacity(0.8)
                      : Colors.white.withOpacity(0.06),
                  width: 1.5,
                ),
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    DateFormat('E').format(date).toUpperCase(),
                    style: GoogleFonts.inter(
                      fontSize: 10,
                      fontWeight: FontWeight.bold,
                      color: isSelected
                          ? Colors.white
                          : AppTheme.textSecondary.withOpacity(0.8),
                    ),
                  ),
                  const SizedBox(height: 6),
                  Text(
                    date.day.toString(),
                    style: GoogleFonts.outfit(
                      fontSize: 20,
                      fontWeight: FontWeight.bold,
                      color: Colors.white,
                    ),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}
