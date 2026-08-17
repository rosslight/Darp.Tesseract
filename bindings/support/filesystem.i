/* Represent filesystem paths as managed strings. The native path remains an
 * automatic C++ value and no path object crosses the ABI boundary. */
%typemap(ctype) const std::filesystem::path& "const char *"
%typemap(imtype) const std::filesystem::path& "string"
%typemap(cstype) const std::filesystem::path& "string"
%typemap(csin) const std::filesystem::path& "$csinput"
%typemap(in) const std::filesystem::path& (std::filesystem::path path)
{
  if ($input == nullptr)
  {
    SWIG_CSharpSetPendingExceptionArgument(
      SWIG_CSharpArgumentNullException, "Path cannot be null.", nullptr);
    return $null;
  }
  path = std::filesystem::u8path($input);
  $1 = &path;
}
